using System.Runtime.InteropServices;

namespace Novolis.Windows.Display;

/// <summary>Enumerates physical monitors and their virtual-screen bounds.</summary>
public sealed class WindowsDisplayTopology
{
    /// <summary>Returns the current monitor topology in stable enumeration order.</summary>
    public IReadOnlyList<WindowsMonitorInfo> GetMonitors()
    {
        var monitors = new List<WindowsMonitorInfo>();
        EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            (monitor, _, _, _) =>
            {
                var info = new MonitorInfo
                {
                    Size = Marshal.SizeOf<MonitorInfo>(),
                };
                if (!GetMonitorInfo(monitor, ref info))
                    return true;

                var width = info.Monitor.Right - info.Monitor.Left;
                var height = info.Monitor.Bottom - info.Monitor.Top;
                monitors.Add(new WindowsMonitorInfo(
                    info.DeviceName,
                    info.Monitor.Left,
                    info.Monitor.Top,
                    width,
                    height,
                    96));
                return true;
            },
            IntPtr.Zero);
        return monitors;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayMonitors(
        IntPtr deviceContext,
        IntPtr clip,
        MonitorEnumProc callback,
        IntPtr data);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

    private delegate bool MonitorEnumProc(
        IntPtr monitor,
        IntPtr deviceContext,
        IntPtr rectangle,
        IntPtr data);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MonitorInfo
    {
        public int Size;
        public Rect Monitor;
        public Rect Work;
        public uint Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
