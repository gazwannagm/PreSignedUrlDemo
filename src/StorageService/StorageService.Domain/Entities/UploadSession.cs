using StorageService.Domain.ValueObjects;

namespace StorageService.Domain.Entities;

public class UploadSession
{
    public string UploadId { get; set; }
    public FileMetadata Metadata { get; set; }
    public string Signature { get; set; }
    public long ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public UploadSession()
    {
        UploadId = Guid.NewGuid().ToString();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsExpired()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return now > ExpiresAt;
    }
}