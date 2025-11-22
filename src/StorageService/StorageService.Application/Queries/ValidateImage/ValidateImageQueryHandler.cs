using MediatR;
using Microsoft.Extensions.Logging;
using StorageService.Domain.Repositories;

namespace StorageService.Application.Queries.ValidateImage;

public class ValidateImageQueryHandler(IImageRepository imageRepository, ILogger<ValidateImageQueryHandler> logger)
    : IRequestHandler<ValidateImageQuery, ValidateImageResult?>
{
    public async Task<ValidateImageResult?> Handle(ValidateImageQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating image: {ImageId}", request.ImageId);

        var image = await imageRepository.GetByIdAsync(request.ImageId);
        
        if (image == null)
        {
            logger.LogWarning("Image not found: {ImageId}", request.ImageId);
            return null;
        }

        return new ValidateImageResult
        {
            ImageId = image.ImageId,
            FileName = image.FileName,
            FileSize = image.FileSize,
            UploadedAt = image.UploadedAt
        };
    }
}