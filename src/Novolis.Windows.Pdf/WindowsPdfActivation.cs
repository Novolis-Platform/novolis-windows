using Microsoft.Windows.AppLifecycle;
using Novolis.Pdf.Abstractions;
using Novolis.Pdf.Platform;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace Novolis.Windows.Pdf;

/// <summary>Converts Windows file activation into platform-neutral PDF requests.</summary>
public static class WindowsPdfActivation
{
    /// <summary>Creates a request from a Windows file activation, if it contains a PDF.</summary>
    public static PdfOpenRequest? TryCreateRequest(AppActivationArguments? activation)
    {
        if (activation?.Kind != ExtendedActivationKind.File
            || activation.Data is not IFileActivatedEventArgs fileArgs
            || fileArgs.Files.OfType<StorageFile>().FirstOrDefault() is not { } file
            || !IsPdf(file.Name))
            return null;

        return new PdfOpenRequest(
            new PdfSourceDescriptor(file.Name, file.Path),
            async cancellationToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var randomAccessStream = await file.OpenAsync(FileAccessMode.Read);
                return randomAccessStream.AsStreamForRead();
            });
    }

    /// <summary>Creates a request from a command-line PDF path, if present.</summary>
    public static PdfOpenRequest? TryCreateRequest(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var path = arguments
            .Select(static value => value.Trim().Trim('"'))
            .FirstOrDefault(static value => IsPdf(value) && File.Exists(value));
        if (path is null)
            return null;

        var fullPath = Path.GetFullPath(path);
        return new PdfOpenRequest(
            new PdfSourceDescriptor(
                Path.GetFileName(fullPath),
                fullPath,
                new FileInfo(fullPath).Length),
            cancellationToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Stream stream = new FileStream(
                    fullPath,
                    new FileStreamOptions
                    {
                        Mode = FileMode.Open,
                        Access = FileAccess.Read,
                        Share = FileShare.ReadWrite | FileShare.Delete,
                        Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                    });
                return ValueTask.FromResult(stream);
            });
    }

    private static bool IsPdf(string? path) =>
        !string.IsNullOrWhiteSpace(path)
        && path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
}
