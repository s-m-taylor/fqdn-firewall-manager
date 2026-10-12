using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FqdnFirewallManager.Models;

namespace FqdnFirewallManager.Services;

/// <summary>
/// Wraps the NetSecurity PowerShell cmdlets (Get/New/Remove-NetFirewallDynamicKeywordAddress,
/// Get-NetFirewallRule) by invoking the built-in Windows PowerShell host.
/// Keeps the app free of extra NuGet dependencies.
/// </summary>
public partial class FirewallService
{
    [GeneratedRegex(@"^(?=.{1,253}$)(\*\.)?([a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,63}$")]
    private static partial Regex FqdnRegex();

    public static bool IsValidFqdn(string value) => FqdnRegex().IsMatch(value.Trim());

    /// <summary>Splits user input on newlines, commas, semicolons and whitespace.</summary>
    public static List<string> ParseInput(string text) =>
        text.Split(new[] { '\r', '\n', ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public async Task<List<DynamicKeywordEntry>> GetEntriesAsync()
    {
        const string script = @"
$ErrorActionPreference = 'Stop'
$dk = @(Get-NetFirewallDynamicKeywordAddress -All -ErrorAction SilentlyContinue | Where-Object { $_.AutoResolve })
$rules = @(Get-NetFirewallRule -ErrorAction SilentlyContinue)
$map = @{}
foreach ($r in $rules) {
  $af = $r | Get-NetFirewallAddressFilter -ErrorAction SilentlyContinue
  foreach ($a in @($af.RemoteDynamicKeywordAddresses)) {
    if (-not $a) { continue }
    if (-not $map.ContainsKey($a)) { $map[$a] = @() }
    $map[$a] += $r.DisplayName
  }
}
$out = foreach ($d in $dk) {
  [pscustomobject]@{
    Id = $d.Id; Keyword = $d.Keyword; Addresses = [string]$d.Addresses; AutoResolve = [bool]$d.AutoResolve
    UsedBy = @($map[$d.Id])
  }
}
ConvertTo-Json -InputObject @($out) -Depth 4 -Compress
";
        var json = await RunPowerShellAsync(script);
        var list = new List<DynamicKeywordEntry>();
        if (string.IsNullOrWhiteSpace(json)) return list;

        using var doc = JsonDocument.Parse(json);
        foreach (var el in doc.RootElement.EnumerateArray())
        {
            var e = new DynamicKeywordEntry
            {
                Id = el.GetProperty("Id").GetString() ?? "",
                Keyword = el.GetProperty("Keyword").GetString() ?? "",
                Addresses = el.TryGetProperty("Addresses", out var ad) ? ad.GetString() ?? "" : "",
                AutoResolve = el.TryGetProperty("AutoResolve", out var ar) && ar.ValueKind == JsonValueKind.True,
            };
            if (el.TryGetProperty("UsedBy", out var ub) && ub.ValueKind == JsonValueKind.Array)
                e.UsedByRules = ub.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString()!)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct().ToList();
            list.Add(e);
        }
        return list;
    }

    /// <summary>
    /// Merges the desired FQDN list with the existing AutoResolve keywords:
    /// adds missing ones, and (optionally) removes ones not in the list that no rule uses.
    /// </summary>
    public async Task<ApplyResult> ApplyAsync(IEnumerable<string> desired, IReadOnlyList<DynamicKeywordEntry> existing, bool removeMissing)
    {
        var result = new ApplyResult();
        var valid = new List<string>();
        foreach (var f in desired)
        {
            if (IsValidFqdn(f)) valid.Add(f);
            else result.Rejected.Add(f);
        }

        var existingByKeyword = existing.ToDictionary(e => e.Keyword.ToLowerInvariant(), e => e);

        foreach (var f in valid)
        {
            if (existingByKeyword.ContainsKey(f)) { result.Unchanged.Add(f); continue; }
            var id = "{" + Guid.NewGuid().ToString() + "}";
            try
            {
                await RunPowerShellAsync(
                    $"New-NetFirewallDynamicKeywordAddress -Id '{id}' -Keyword '{Esc(f)}' -AutoResolve $true -ErrorAction Stop | Out-Null");
                result.Added.Add(f);
            }
            catch (Exception ex) { result.Errors.Add($"{f}: {ex.Message}"); }
        }

        if (removeMissing)
        {
            var keep = new HashSet<string>(valid, StringComparer.OrdinalIgnoreCase);
            foreach (var e in existing.Where(e => !keep.Contains(e.Keyword)))
            {
                if (e.UsedByRules.Count > 0)
                {
                    result.Errors.Add($"{e.Keyword}: kept, still used by {e.UsedByText}");
                    continue;
                }
                try
                {
                    await RemoveAsync(e.Id);
                    result.Removed.Add(e.Keyword);
                }
                catch (Exception ex) { result.Errors.Add($"{e.Keyword}: {ex.Message}"); }
            }
        }
        return result;
    }

    public Task RemoveAsync(string id) =>
        RunPowerShellAsync($"Remove-NetFirewallDynamicKeywordAddress -Id '{Esc(id)}' -ErrorAction Stop");

    private static string Esc(string s) => s.Replace("'", "''");

    private static async Task<string> RunPowerShellAsync(string script)
    {
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(
            "$ProgressPreference='SilentlyContinue';" + script));

        var psi = new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe"),
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encoded}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
        };

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Could not start PowerShell.");
        var stdout = p.StandardOutput.ReadToEndAsync();
        var stderr = p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        var err = (await stderr).Trim();
        if (p.ExitCode != 0 || (err.Length > 0 && !err.StartsWith("#< CLIXML")))
            throw new InvalidOperationException(err.Length > 0 ? err : $"PowerShell exited with code {p.ExitCode}");
        return (await stdout).Trim();
    }
}
