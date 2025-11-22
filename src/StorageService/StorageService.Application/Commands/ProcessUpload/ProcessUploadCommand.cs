using MediatR;

namespace StorageService.Application.Commands.ProcessUpload;

public class ProcessUploadCommand : IRequest<ProcessUploadResult>
{
    public string UploadId { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
}

public class ProcessUploadResult
{
    public string ImageId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}