using MediatR;

namespace StorageService.Application.Queries.ValidateImage;

public class ValidateImageQuery : IRequest<ValidateImageResult?>
{
    public string ImageId { get; set; } = string.Empty;
}



public class ValidateImageResult
{
    public string ImageId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}