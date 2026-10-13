# FQDN Firewall Manager website integration

Destination: `https://sedrictaylor.com/fqdn-firewall-manager`

The editable website is the Lovable project **Sedric's Infinite Mind**
(`8d9cd4c5-55f8-40e0-9af9-64e47335080f`). Cross-project snapshots are
read-only; this repository cannot modify or publish that website.

## Page content

**FQDN Firewall Manager**

Manage Windows Firewall rules by domain name. This free, open-source Windows
tool lets you list automatically resolved domain entries, see which firewall
rules use them, add domains in bulk, and remove unused entries. Entries still
referenced by a rule are protected from removal.

Requirements: Windows 10 1809+ or Windows 11; administrator access. The
self-contained Windows x64 download does not need a separately installed
.NET runtime.

Actions (on the subpage only): **Download for Windows** and **View on GitHub**.

Directly under **Download for Windows**, display:

> Code signing provided by the SignPath Foundation.

Link **SignPath Foundation** to `https://signpath.org/`.

Link structure: the sedrictaylor.com home page has only a node linking to
`/fqdn-firewall-manager`. The direct download link appears only on that
subpage, never on the home page.

- Repository: `https://github.com/s-m-taylor/fqdn-firewall-manager`
- Direct download:
  `https://github.com/s-m-taylor/fqdn-firewall-manager/releases/latest/download/FqdnFirewallManager.exe`

Do not activate a download button before a published release containing that
asset returns a successful response. The release workflow requires SignPath
approval and configured signing secrets/policy.

## Website node

Reuse the existing `Destination` entry structure and its drag physics,
connections, circular image container, reveal-on-focus label, and accessible
link behavior. Add an internal destination with:

- id: `fqdn-firewall`
- label: `FQDN Firewall`
- description: `Windows Firewall Manager`
- href: `/fqdn-firewall-manager`
- icon: `fqdn-firewall`
- internal: `true`
- initial home: `{ x: 0.84, y: 0.48 }` (verify overlap at different sizes)

Extend the icon union and existing image-icon branch. Use the shield logo
from `website-assets/fqdn-firewall-logo.png` in the existing `node-icon-logo`
container. Keep the alternative gateway logo as an unused design option.

## Browser icon

Use the shield-derived PNG/ICO browser icons in `website-assets`. Apply them
to this tool page's head, not the portfolio's global icon, unless explicitly
requested otherwise. Verify the actual browser favicon resolves to the tool
icon on this page and the portfolio icon on other pages.

## Outstanding access

- Open the website project to implement and publish its page and node.
- Confirm the exact GitHub repository URL and public visibility.
- Repository settings → General → Danger Zone → Change visibility → Public
  if needed; review the repository for secrets before exposing it.
- Configure SignPath, then push a version tag or run the release workflow
  against an existing version tag. A successful release provides the download.