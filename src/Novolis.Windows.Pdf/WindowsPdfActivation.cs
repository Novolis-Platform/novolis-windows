using Novolis.Pdf.Abstractions;
using Novolis.Pdf.Platform;

namespace Novolis.Windows.Pdf;

/// <summary>Converts Windows file activation into platform-neutral PDF requests.</summary>
public static class WindowsPdfActivation
{
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
        return CreateLocalFileRequest(fullPath);
    }

    /// <summary>Creates a request for a local PDF path selected by the host.</summary>
    public static PdfOpenRequest CreateLocalFileRequest(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        if (!IsPdf(fullPath))
            throw new ArgumentException("The selected file is not a PDF.", nameof(path));
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

    /// <summary>Creates a request around a host-provided Windows stream.</summary>
    public static PdfOpenRequest CreateRequest(
        PdfSourceDescriptor descriptor,
        Func<CancellationToken, ValueTask<Stream>> openReadAsync) =>
        new(
            (descriptor ?? throw new ArgumentNullException(nameof(descriptor))).Normalize(),
            openReadAsync ?? throw new ArgumentNullException(nameof(openReadAsync)));

    private static bool IsPdf(string? path) =>
        !string.IsNullOrWhiteSpace(path)
        && path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
}
