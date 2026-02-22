namespace Y.Authentication.Domain.ValueObjects;
public sealed class FileUpload
{
    public Guid BlobId { get; set; }
    public string Url { get; set; }
    public string Mime { get; set; }
    public string Extension { get; set; }
    public string Description { get; set; } = string.Empty;

    public FileUpload(
        Guid blobId,
        string url,
        string mime,
        string extension)
    {
        BlobId = blobId;
        Url = url;
        Mime = mime;
        Extension = extension;
    }
}

