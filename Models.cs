using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FqdnFirewallManager.Models;

/// <summary>
/// One Dynamic Keyword Address (AutoResolve / FQDN) as reported by
/// Get-NetFirewallDynamicKeywordAddress, plus the firewall rules that use it.
/// </summary>
public class DynamicKeywordEntry : INotifyPropertyChanged
{
    public string Id { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
    public string Addresses { get; set; } = string.Empty;
    public bool AutoResolve { get; set; }

    /// <summary>Display names of firewall rules referencing this keyword ID.</summary>
    public List<string> UsedByRules { get; set; } = new();

    public string UsedByText => UsedByRules.Count == 0
        ? "(not used by any rule)"
        : string.Join(", ", UsedByRules);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Result of a validate/apply pass over the user's input text.
/// </summary>
public class ApplyResult
{
    public List<string> Added { get; } = new();
    public List<string> Removed { get; } = new();
    public List<string> Unchanged { get; } = new();
    public List<string> Rejected { get; } = new();
    public List<string> Errors { get; } = new();

    public bool HasErrors => Errors.Count > 0;
}
