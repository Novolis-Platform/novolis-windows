namespace Novolis.Windows.Display;

/// <summary>One physical monitor in the Windows display topology.</summary>
public sealed record WindowsMonitorInfo(
    string DeviceName,
    int Left,
    int Top,
    int Width,
    int Height,
    uint Dpi);
