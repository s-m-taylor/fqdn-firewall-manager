# WinGet manifests

Templates used by the release workflow. Each release run produces a
`winget-manifests-vX.Y.Z` artifact (Actions → run → Artifacts) with the
version, download URL and SHA-256 filled in.

## Submitting a version

1. Download and unzip the `winget-manifests-vX.Y.Z` artifact.
2. Fork https://github.com/microsoft/winget-pkgs.
3. Copy the `manifests/s/SedricTaylor/FqdnFirewallManager/<version>` folder
   into the same path in your fork and open a pull request.
4. Optional local check first: `winget validate <folder>` and
   `winget install --manifest <folder>` (needs `winget settings --enable LocalManifestFiles` as admin).

Once merged, users can install with:

```powershell
winget install SedricTaylor.FqdnFirewallManager
```
