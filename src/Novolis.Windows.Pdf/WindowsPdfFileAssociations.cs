using Microsoft.Win32;

namespace Novolis.Windows.Pdf;

/// <summary>Registers PDF Open With metadata in the current user hive.</summary>
public sealed class WindowsPdfFileAssociations
{
    /// <summary>Registers the supplied executable for PDF Open With.</summary>
    public void Register(
        string executablePath,
        WindowsPdfAssociationOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        options ??= new WindowsPdfAssociationOptions();
        var executable = Path.GetFullPath(executablePath);
        if (!File.Exists(executable))
            throw new FileNotFoundException("The PDF reader executable was not found.", executable);

        using var classes = Registry.CurrentUser.CreateSubKey(
            @"Software\Classes",
            writable: true);
        using var progId = classes.CreateSubKey(options.ProgId, writable: true);
        progId.SetValue(null, options.DisplayName);
        using (var icon = progId.CreateSubKey("DefaultIcon", writable: true))
            icon.SetValue(null, $"\"{executable}\",0");
        using (var command = progId.CreateSubKey(@"shell\open\command", writable: true))
            command.SetValue(null, $"\"{executable}\" \"%1\"");

        using var openWith = classes.CreateSubKey(
            $@"{options.Extension}\OpenWithProgids",
            writable: true);
        openWith.SetValue(options.ProgId, string.Empty, RegistryValueKind.String);
    }

    /// <summary>Removes current-user association metadata.</summary>
    public void Unregister(WindowsPdfAssociationOptions? options = null)
    {
        options ??= new WindowsPdfAssociationOptions();
        using var classes = Registry.CurrentUser.CreateSubKey(
            @"Software\Classes",
            writable: true);
        try
        {
            classes.DeleteSubKeyTree(options.ProgId, throwOnMissingSubKey: false);
        }
        catch (ArgumentException)
        {
        }

        try
        {
            using var openWith = classes.OpenSubKey(
                $@"{options.Extension}\OpenWithProgids",
                writable: true);
            openWith?.DeleteValue(options.ProgId, throwOnMissingValue: false);
        }
        catch (ArgumentException)
        {
        }
    }
}
