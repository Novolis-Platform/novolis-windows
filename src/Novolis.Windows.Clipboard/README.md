# Novolis.Windows.Clipboard

Unicode text clipboard access for a Windows interactive session.

File and image formats are intentionally separate capabilities and are not
silently converted to text.

## Install

```xml
<PackageReference Include="Novolis.Windows.Clipboard" Version="2026.1.*" />
```

## Usage

Use `WindowsClipboardService` from the interactive session that owns the
clipboard.
