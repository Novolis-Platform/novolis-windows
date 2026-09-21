using System.Runtime.InteropServices;
using System.Text;

namespace Novolis.Windows.Sessions;

/// <summary>Reads the active Windows interactive session without owning a user interface.</summary>
public sealed class WindowsSessionManager
{
    private const uint CurrentServer = 0;
    private const uint InvalidSessionId = 0xFFFFFFFF;

    /// <summary>Gets the active console session id, or <see langword="null"/> when no console exists.</summary>
    public uint? TryGetActiveConsoleSessionId()
    {
        var sessionId = WtsGetActiveConsoleSessionId();
        return sessionId == InvalidSessionId ? null : sessionId;
    }

    /// <summary>Reads the active console session and its user identity.</summary>
    public bool TryGetActiveSession(out WindowsSessionInfo? session)
    {
        session = null;
        var sessionId = TryGetActiveConsoleSessionId();
        if (sessionId is null)
            return false;

        var userName = QueryString(sessionId.Value, WtsInfoClass.UserName);
        if (string.IsNullOrWhiteSpace(userName))
            return false;

        var domainName = QueryString(sessionId.Value, WtsInfoClass.DomainName) ?? string.Empty;
        var state = QueryState(sessionId.Value);
        session = new WindowsSessionInfo(
            sessionId.Value,
            userName,
            domainName,
            state == WtsConnectState.Active);
        return session.IsActive;
    }

    /// <summary>
    /// Starts a helper executable inside the active user's interactive session.
    /// The child is created without a visible console window.
    /// </summary>
    public bool TryStartInActiveSession(
        string executablePath,
        string arguments,
        out int errorCode)
    {
        ArgumentNullException.ThrowIfNull(executablePath);
        ArgumentNullException.ThrowIfNull(arguments);
        errorCode = 0;
        if (!File.Exists(executablePath))
        {
            errorCode = unchecked((int)0x80070002);
            return false;
        }

        var sessionId = TryGetActiveConsoleSessionId();
        if (sessionId is null
            || !WtsQueryUserToken(sessionId.Value, out var impersonationToken)
            || impersonationToken == IntPtr.Zero)
        {
            errorCode = Marshal.GetLastWin32Error();
            return false;
        }

        try
        {
            if (!DuplicateTokenEx(
                    impersonationToken,
                    TokenAllAccess,
                    IntPtr.Zero,
                    SecurityImpersonation,
                    TokenPrimary,
                    out var primaryToken)
                || primaryToken == IntPtr.Zero)
            {
                errorCode = Marshal.GetLastWin32Error();
                return false;
            }

            try
            {
                var environment = IntPtr.Zero;
                try
                {
                    CreateEnvironmentBlock(out environment, primaryToken, false);
                    var startup = new StartupInfo
                    {
                        Size = Marshal.SizeOf<StartupInfo>(),
                        Desktop = @"winsta0\default",
                        ShowWindow = 0,
                    };
                    var commandLine = new StringBuilder(
                        $"\"{executablePath}\" {arguments}".TrimEnd());
                    var created = CreateProcessAsUser(
                        primaryToken,
                        executablePath,
                        commandLine,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        false,
                        CreateUnicodeEnvironment | CreateNoWindow,
                        environment,
                        Path.GetDirectoryName(executablePath),
                        ref startup,
                        out var process);
                    if (!created)
                    {
                        errorCode = Marshal.GetLastWin32Error();
                        return false;
                    }

                    CloseHandle(process.Process);
                    CloseHandle(process.Thread);
                    return true;
                }
                finally
                {
                    if (environment != IntPtr.Zero)
                        DestroyEnvironmentBlock(environment);
                }
            }
            finally
            {
                CloseHandle(primaryToken);
            }
        }
        finally
        {
            CloseHandle(impersonationToken);
        }
    }

    private static string? QueryString(uint sessionId, WtsInfoClass infoClass)
    {
        if (!WtsQuerySessionInformation(
                CurrentServer,
                sessionId,
                infoClass,
                out var buffer,
                out var byteCount)
            || buffer == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            return Marshal.PtrToStringUni(buffer, checked((int)byteCount / 2))?.TrimEnd('\0');
        }
        finally
        {
            WtsFreeMemory(buffer);
        }
    }

    private static WtsConnectState QueryState(uint sessionId)
    {
        if (!WtsQuerySessionInformation(
                CurrentServer,
                sessionId,
                WtsInfoClass.ConnectState,
                out var buffer,
                out _)
            || buffer == IntPtr.Zero)
        {
            return WtsConnectState.Disconnected;
        }

        try
        {
            return (WtsConnectState)Marshal.ReadInt32(buffer);
        }
        finally
        {
            WtsFreeMemory(buffer);
        }
    }

    [DllImport("kernel32.dll")]
    private static extern uint WtsGetActiveConsoleSessionId();

    [DllImport("Wtsapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WtsQuerySessionInformation(
        uint server,
        uint sessionId,
        WtsInfoClass infoClass,
        out IntPtr buffer,
        out uint byteCount);

    [DllImport("Wtsapi32.dll")]
    private static extern void WtsFreeMemory(IntPtr buffer);

    [DllImport("Wtsapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WtsQueryUserToken(uint sessionId, out IntPtr token);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateTokenEx(
        IntPtr existingToken,
        uint desiredAccess,
        IntPtr tokenAttributes,
        int impersonationLevel,
        int tokenType,
        out IntPtr primaryToken);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateProcessAsUser(
        IntPtr token,
        string applicationName,
        StringBuilder commandLine,
        IntPtr processAttributes,
        IntPtr threadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandles,
        uint creationFlags,
        IntPtr environment,
        string? currentDirectory,
        ref StartupInfo startupInfo,
        out ProcessInformation processInformation);

    [DllImport("userenv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateEnvironmentBlock(
        out IntPtr environment,
        IntPtr token,
        [MarshalAs(UnmanagedType.Bool)] bool inherit);

    [DllImport("userenv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyEnvironmentBlock(IntPtr environment);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    private const uint TokenAllAccess = 0x000F01FF;
    private const int SecurityImpersonation = 2;
    private const int TokenPrimary = 1;
    private const uint CreateUnicodeEnvironment = 0x00000400;
    private const uint CreateNoWindow = 0x08000000;

    private enum WtsInfoClass
    {
        ConnectState = 8,
        UserName = 5,
        DomainName = 7,
    }

    private enum WtsConnectState
    {
        Active = 0,
        Connected = 1,
        ConnectQuery = 2,
        Shadow = 3,
        Disconnected = 4,
        Idle = 5,
        Listen = 6,
        Reset = 7,
        Down = 8,
        Init = 9,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        public int Size;
        public string? Reserved;
        public string? Desktop;
        public string? Title;
        public int X;
        public int Y;
        public int XSize;
        public int YSize;
        public int XCountChars;
        public int YCountChars;
        public int FillAttribute;
        public int Flags;
        public short ShowWindow;
        public short Reserved2;
        public IntPtr Reserved2Pointer;
        public IntPtr StandardInput;
        public IntPtr StandardOutput;
        public IntPtr StandardError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr Process;
        public IntPtr Thread;
        public uint ProcessId;
        public uint ThreadId;
    }
}
