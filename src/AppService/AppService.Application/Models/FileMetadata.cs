namespace AppService.Application.Models;

public class FileMetadata
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public int ExpiresIn { get; set; }
}