namespace StorageService.Domain.ValueObjects;

public class FileMetadata
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public int ExpiresIn { get; set; }

    public bool IsExpired()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Timestamp < now - 300; // 5 minutes tolerance
    }

    public bool ValidateFileSize(long actualSize)
    {
        return actualSize == FileSize;
    }
}