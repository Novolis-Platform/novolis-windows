<!-- novolis-marketing:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-brand-transparent.svg" width="360" alt="Novolis"/>
  </a>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/banners/novolis-windows.svg" width="100%" alt="novolis-windows"/>
</p>

<p align="center">
  <strong>Windows capability libraries</strong><br/>
  Operating-system primitives for Novolis hosts.
</p>

<p align="center">
  <a href="https://novolis-platform.github.io/.github/novolis-template-dotnet/"><img src="https://img.shields.io/badge/docs-portfolio-0a7ea3" alt="docs"/></a>
  <a href="https://github.com/Novolis-Platform/novolis-windows/actions"><img src="https://img.shields.io/github/actions/workflow/status/Novolis-Platform/novolis-windows/merge.yml?branch=main&label=merge&logo=github" alt="merge"/></a>
  <a href="https://github.com/orgs/Novolis-Platform/packages?repo_name=novolis-template-dotnet"><img src="https://img.shields.io/badge/packages-GitHub%20Packages-0a7ea3?logo=nuget" alt="packages"/></a>
  <a href="https://github.com/Novolis-Platform"><img src="https://img.shields.io/badge/org-Novolis--Platform-111827" alt="org"/></a>
</p>

<p align="center">
  <a href="https://novolis-platform.github.io/.github/novolis-windows/">Docs</a>
  ·
  <a href="https://nuget.pkg.github.com/Novolis-Platform/index.json"><code>https://nuget.pkg.github.com/Novolis-Platform/index.json</code></a>
  ·
  <a href="https://github.com/Novolis-Platform/.github/blob/main/profile/README.md">Org landing</a>
  ·
  <a href="https://github.com/Novolis-Platform/novolis-governance">Governance</a>
</p>

---
<!-- novolis-marketing:end -->
# novolis-windows

Windows capability libraries for Novolis hosts.

The packages in this repository expose operating-system capabilities without
product protocol or user-interface concepts. Reach uses them from its Windows
host; other Windows products may use them independently.

## Packages

| Package | Capability |
|---------|------------|
| `Novolis.Windows.Sessions` | Active interactive session discovery |
| `Novolis.Windows.Input` | Pointer, keyboard, and Unicode input injection |
| `Novolis.Windows.Clipboard` | Unicode text clipboard access |
| `Novolis.Windows.Display` | Monitor topology and virtual-screen bounds |
| `Novolis.Windows.Audio` | WASAPI loopback capture |

## Restore

Restore uses nuget.org and GitHub Packages only. Local multi-repo iteration
uses `d:\novolis\Novolis.Platform.slnx` ProjectReference mode.

See [getting started](docs/getting-started.md), [design](docs/design.md), and
[release](docs/release.md).

