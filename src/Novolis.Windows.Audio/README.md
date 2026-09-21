# Novolis.Windows.Audio

WASAPI loopback capture for Windows interactive-session hosts.

The package exposes PCM blocks from the default render endpoint. Encoding and
wire transport remain product concerns.

## Install

```xml
<PackageReference Include="Novolis.Windows.Audio" Version="2026.1.*" />
```

## Usage

Start `WindowsLoopbackAudioCapture` from the interactive host and consume
its PCM data events.
