using MediatR;
using Microsoft.AspNetCore.Mvc;
using StorageService.Api.Models.Responses;
using StorageService.Application.Queries.ValidateImage;
using StorageService.Domain.Repositories;

namespace StorageService.Api.Controllers;

[ApiController]
[Route("internal/images")]
[Produces("application/json")]
public class ImageController(ISender sender, IImageRepository imageRepository, ILogger<ImageController> logger)
    : ControllerBase
{
    [HttpGet("{imageId}/validate")]
    public async Task<ActionResult<ImageValidationResponse>> ValidateImage(string imageId)
    {
        logger.LogInformation("Validating image ID: {ImageId}", imageId);

        try
        {
            var query = new ValidateImageQuery { ImageId = imageId };
            var result = await sender.Send(query);

            if (result == null)
            {
                return NotFound();
            }

            var response = new ImageValidationResponse
            {
                ImageId = result.ImageId,
                FileName = result.FileName,
                FileSize = result.FileSize,
                UploadedAt = result.UploadedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating image");
            return StatusCode(500, new { error = "Failed to validate image" });
        }
    }

    [HttpGet("/images/{imageId}")]
    public async Task<IActionResult> GetImage(string imageId)
    {
        var image = await imageRepository.GetByIdAsync(imageId);
        if (image == null)
        {
            return NotFound();
        }

        return File(image.Data, image.ContentType, image.FileName);
    }
}