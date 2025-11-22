using MediatR;
using StorageService.Domain.ValueObjects;

namespace StorageService.Application.Commands.CreateUploadSession;

public class CreateUploadSessionCommand : IRequest<CreateUploadSessionResult>
{
    public FileMetadata Metadata { get; set; }
    public string Signature { get; set; } = string.Empty;
}


public class CreateUploadSessionResult
{
    public string UploadUrl { get; set; } = string.Empty;
    public string UploadId { get; set; } = string.Empty;
    public long ExpiresAt { get; set; }
}