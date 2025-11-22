using AppService.Api.Models.Requests;
using AppService.Api.Models.Responses;
using AppService.Api.Validators;
using AppService.Application.Commands.RequestUploadUrl;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UploadController(
    ISender sender,
    ILogger<UploadController> logger)
    : ControllerBase
{
    [HttpPost("request")]
    public async Task<ActionResult<PresignedUrlResponse>> RequestUploadUrl([FromBody] RequestUploadUrlRequest request)
    {
        logger.LogInformation("Received upload request for file: {FileName}, Size: {FileSize}, Type: {ContentType}",
            request.FileName, request.FileSize, request.ContentType);

        // Validate request
        var validator = new RequestUploadUrlValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var command = new RequestUploadUrlCommand
        {
            FileName = request.FileName,
            FileSize = request.FileSize,
            ContentType = request.ContentType
        };

        var result = await sender.Send(command);

        var response = new PresignedUrlResponse
        {
            UploadUrl = result.UploadUrl,
            UploadId = result.UploadId,
            ExpiresAt = new DateTimeOffset(result.ExpiresAt).ToUnixTimeSeconds()
        };

        logger.LogInformation("Successfully generated upload URL with upload ID: {UploadId}", response.UploadId);

        return Ok(response);
    }
}