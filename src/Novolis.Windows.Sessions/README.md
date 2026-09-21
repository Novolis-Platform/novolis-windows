# Novolis.Windows.Sessions

Windows session discovery for hosts that need to attach work to the logged-in
interactive user session.

The package has no product or remote-control concepts. It reports the active
session and user identity; a host decides how to use that information.

## Install

```xml
<PackageReference Include="Novolis.Windows.Sessions" Version="2026.1.*" />
```

## Usage

Use `WindowsSessionManager` to query the active interactive session before
starting session-bound work.
