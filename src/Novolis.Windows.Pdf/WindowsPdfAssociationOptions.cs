namespace Novolis.Windows.Pdf;

/// <summary>Current-user Windows association settings for PDF files.</summary>
public sealed record WindowsPdfAssociationOptions(
    string ProgId = "Novolis.PdfReader",
    string DisplayName = "Novolis PDF Reader",
    string Extension = ".pdf");
