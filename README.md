# novolis-windows

Windows capability libraries for Novolis hosts.

The packages in this repository expose operating-system capabilities without
product protocol or user-interface concepts. Reach uses them from its Windows
host; other Windows products may use them independently.

## Packages

| Package | Capability |
| --- | --- |
| `Novolis.Windows.Sessions` | Active interactive session discovery |
| `Novolis.Windows.Input` | Pointer, keyboard, and Unicode input injection |
| `Novolis.Windows.Clipboard` | Unicode text and file-list clipboard access |
| `Novolis.Windows.Display` | Monitor topology and virtual-screen bounds |
| `Novolis.Windows.Audio` | WASAPI loopback capture |

## Restore

Restore uses nuget.org and GitHub Packages only. Local multi-repo iteration
uses `d:\novolis\Novolis.Platform.slnx` ProjectReference mode.

See [getting started](docs/getting-started.md), [design](docs/design.md), and
[release](docs/release.md).
