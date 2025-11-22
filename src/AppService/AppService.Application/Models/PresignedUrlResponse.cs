namespace AppService.Application.Models;

public class PresignedUrlResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public string UploadId { get; set; } = string.Empty;
    public long ExpiresAt { get; set; }
}