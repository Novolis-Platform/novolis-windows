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
        var parts = arguments
            .Select(static value => value.Trim().Trim('"'))
            .Where(static value => value.Length > 0)
            .ToArray();
        if (FindExistingPdf(parts) is not { } path)
            return null;

        return CreateLocalFileRequest(Path.GetFullPath(path));
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

    private static string? FindExistingPdf(IReadOnlyList<string> parts)
    {
        foreach (var part in parts)
        {
            if (IsPdf(part) && File.Exists(part))
                return part;
        }

        for (var start = 0; start < parts.Count; start++)
        {
            var combined = parts[start];
            for (var end = start + 1; end < parts.Count; end++)
            {
                combined = $"{combined} {parts[end]}";
                if (IsPdf(combined) && File.Exists(combined))
                    return combined;
            }
        }

        return null;
    }

    private static bool IsPdf(string? path) =>
        !string.IsNullOrWhiteSpace(path)
        && path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
}
