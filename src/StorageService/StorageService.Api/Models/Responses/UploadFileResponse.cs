namespace StorageService.Api.Models.Responses;

public class UploadFileResponse
{
    public string ImageId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}