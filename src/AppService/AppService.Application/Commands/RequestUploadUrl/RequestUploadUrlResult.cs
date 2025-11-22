namespace AppService.Application.Commands.RequestUploadUrl;

public class RequestUploadUrlResult
{
    public string UploadUrl { get; set; } = string.Empty;
    public string UploadId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}