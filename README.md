# FQDN Windows Firewall Manager

A standalone WPF tool for managing Windows Firewall Dynamic Keyword Addresses
(FQDN rules). In a default-deny outbound environment, this tool lets you add,
view, and remove FQDNs used by outbound allow rules for better control.

## Features

- List all AutoResolve dynamic keyword addresses and the rules that use them
- Add FQDNs in bulk (one per line, or comma separated)
- Optionally remove keywords not in your list (skipped if still used by a rule)
- Remove selected keywords with a safety prompt
- Runs elevated automatically (firewall changes require admin rights)

## Requirements

- Windows 10 1809+ / Windows 11
- .NET 10 SDK to build: https://dotnet.microsoft.com/download/dotnet/10.0

## Build

```powershell
dotnet build -c Release
```

Single self-contained exe (no .NET needed on the target machine):

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output: `bin\Release\net10.0-windows\win-x64\publish\FqdnFirewallManager.exe`

See [BUILD.md](BUILD.md) for full instructions.

## Releases

Tagged releases (`v*`) are built automatically by GitHub Actions, signed via
[SignPath Foundation](https://signpath.io/), and attached to a GitHub Release.

## License

[MIT](LICENSE)
