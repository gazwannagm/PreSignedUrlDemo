using StorageService.Domain.ValueObjects;

namespace StorageService.Api.Models.Requests;

public class PresignedUrlRequest
{
    public FileMetadata Metadata { get; set; }
    public string Signature { get; set; } = string.Empty;
}
