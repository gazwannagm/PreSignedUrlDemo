namespace StorageService.Domain.Entities;

public class StoredImage
{
    public string ImageId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public byte[] Data { get; set; }
    public DateTime UploadedAt { get; set; }

    public StoredImage()
    {
        ImageId = Guid.NewGuid().ToString();
        UploadedAt = DateTime.UtcNow;
    }
}