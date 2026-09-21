using System.Collections.Specialized;
using FormsClipboard = System.Windows.Forms.Clipboard;
using System.Windows.Forms;

namespace Novolis.Windows.Clipboard;

/// <summary>Reads and writes Unicode text in the current interactive session.</summary>
public sealed class WindowsClipboardService
{
    /// <summary>Returns current Unicode clipboard text, or <see langword="null"/>.</summary>
    public string? ReadText()
    {
        return FormsClipboard.ContainsText(TextDataFormat.UnicodeText)
            ? FormsClipboard.GetText(TextDataFormat.UnicodeText)
            : null;
    }

    /// <summary>Replaces the current clipboard contents with Unicode text.</summary>
    public void WriteText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        FormsClipboard.SetText(text, TextDataFormat.UnicodeText);
    }

    /// <summary>Returns file paths currently held by the clipboard.</summary>
    public IReadOnlyList<string> ReadFileDropList()
    {
        if (!FormsClipboard.ContainsFileDropList())
            return Array.Empty<string>();

        return FormsClipboard.GetFileDropList().Cast<string>().ToArray();
    }

    /// <summary>Places existing file paths on the clipboard.</summary>
    public void WriteFileDropList(IEnumerable<string> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        var paths = files
            .Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var collection = new StringCollection();
        collection.AddRange(paths);
        FormsClipboard.SetFileDropList(collection);
    }
}
