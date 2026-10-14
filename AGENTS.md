# Project rules

- Keep this repository as the Windows WPF application's source; integrate personal-site pages in the existing website project rather than replacing this app with a web scaffold.
- Distribute the executable as a GitHub Release asset named `FqdnFirewallManager.exe`, not a committed binary, so the website can use GitHub's stable latest-release download URL.
- Keep project logo source images under `src/assets` and browser-ready derivatives under `website-assets` for reuse in the personal website.