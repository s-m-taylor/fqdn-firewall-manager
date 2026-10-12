# FQDN Firewall Manager – build instructions (.NET 10)

These files are **not** a standalone project. They are meant to be dropped
into a WPF project that the .NET SDK has already created, overwriting the
generated files.

## Requirements
- Windows 10 1809+/11 (Dynamic Keyword Addresses need a recent build)
- .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
- Run the finished app as Administrator (the manifest requests it)

## Steps
1. Create the base WPF project:
   ```powershell
   dotnet new wpf -n FqdnFirewallManager -f net10.0
   cd FqdnFirewallManager
   ```
2. Copy **all** files and folders from this zip into that folder,
   choosing **overwrite / replace** when asked
   (`FqdnFirewallManager.csproj`, `App.xaml`, `App.xaml.cs`,
   `MainWindow.xaml`, `MainWindow.xaml.cs`, `app.manifest`,
   `Models\`, `Services\`).
   - Delete `AssemblyInfo.cs` only if the build reports duplicate attributes.
3. Build:
   ```powershell
   dotnet build -c Release
   ```
   Output: `bin\Release\net10.0-windows\FqdnFirewallManager.exe`
   (needs the .NET 10 Desktop Runtime on the target machine).
4. Optional – single self-contained exe (no .NET needed on target):
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```
   Output: `bin\Release\net10.0-windows\win-x64\publish\FqdnFirewallManager.exe`

## Using a different .NET version
Change `<TargetFramework>` in `FqdnFirewallManager.csproj`
(e.g. `net8.0-windows`, `net9.0-windows`) to match your installed SDK.
