using System.Runtime.InteropServices;

namespace Novolis.Windows.Input;

/// <summary>Injects pointer, keyboard, and Unicode text input through Win32.</summary>
public sealed class WindowsInputController
{
    private const uint InputMouse = 0;
    private const uint InputKeyboard = 1;
    private const uint MouseMove = 0x0001;
    private const uint MouseLeftDown = 0x0002;
    private const uint MouseLeftUp = 0x0004;
    private const uint MouseRightDown = 0x0008;
    private const uint MouseRightUp = 0x0010;
    private const uint MouseMiddleDown = 0x0020;
    private const uint MouseMiddleUp = 0x0040;
    private const uint MouseWheel = 0x0800;
    private const uint KeyUp = 0x0002;
    private const uint KeyboardUnicode = 0x0004;

    /// <summary>Moves the pointer to screen coordinates.</summary>
    public bool MovePointer(int x, int y) => SetCursorPos(x, y);

    /// <summary>Clicks a pointer button one or more times.</summary>
    public bool Click(WindowsPointerButton button = WindowsPointerButton.Left, int count = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        var (down, up) = button switch
        {
            WindowsPointerButton.Left => (MouseLeftDown, MouseLeftUp),
            WindowsPointerButton.Right => (MouseRightDown, MouseRightUp),
            WindowsPointerButton.Middle => (MouseMiddleDown, MouseMiddleUp),
            _ => throw new ArgumentOutOfRangeException(nameof(button)),
        };

        var inputs = new Input[count * 2];
        for (var index = 0; index < count; index++)
        {
            inputs[index * 2] = Input.Mouse(down);
            inputs[index * 2 + 1] = Input.Mouse(up);
        }

        return Send(inputs);
    }

    /// <summary>Scrolls the pointer wheel by the requested wheel delta.</summary>
    public bool Scroll(int delta) =>
        Send([Input.Mouse(MouseWheel, unchecked((uint)delta))]);

    /// <summary>Injects a virtual-key press or release.</summary>
    public bool Key(ushort virtualKey, bool release)
    {
        ArgumentOutOfRangeException.ThrowIfZero(virtualKey);
        return Send([Input.Keyboard(virtualKey, release ? KeyUp : 0)]);
    }

    /// <summary>Injects text as Unicode key pairs.</summary>
    public bool Text(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0)
            return true;

        var inputs = new Input[text.Length * 2];
        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            inputs[index * 2] = Input.Unicode(character, 0);
            inputs[index * 2 + 1] = Input.Unicode(character, KeyUp);
        }

        return Send(inputs);
    }

    private static bool Send(Input[] inputs)
    {
        var sent = SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<Input>());
        return sent == inputs.Length;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int inputSize);

    /// <summary>Supported pointer buttons.</summary>
    public enum WindowsPointerButton
    {
        /// <summary>Primary pointer button.</summary>
        Left,

        /// <summary>Secondary pointer button.</summary>
        Right,

        /// <summary>Middle pointer button.</summary>
        Middle,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;

        public static Input Mouse(uint flags, uint data = 0) => new()
        {
            Type = InputMouse,
            Data = new InputUnion
            {
                Mouse = new MouseInput { Flags = flags, MouseData = data },
            },
        };

        public static Input Keyboard(ushort virtualKey, uint flags) => new()
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = virtualKey,
                    Flags = flags,
                },
            },
        };

        public static Input Unicode(char character, uint flags) => new()
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    ScanCode = character,
                    Flags = KeyboardUnicode | flags,
                },
            },
        };
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput Mouse;

        [FieldOffset(0)]
        public KeyboardInput Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }
}
