using MediatR;
using Microsoft.AspNetCore.Mvc;
using StorageService.Api.Models.Requests;
using StorageService.Api.Models.Responses;
using StorageService.Application.Commands.CreateUploadSession;
using StorageService.Application.Commands.ProcessUpload;

namespace StorageService.Api.Controllers;

[ApiController]
[Route("internal")]
[Produces("application/json")]
public class UploadController(ISender sender, ILogger<UploadController> logger) : ControllerBase
{
    [HttpPost("presigned-url")]
    public async Task<ActionResult<PresignedUrlResponse>> GeneratePresignedUrl([FromBody] PresignedUrlRequest request)
    {
        logger.LogInformation("Received pre-signed URL request for file: {FileName}", request.Metadata.FileName);

        try
        {
            // Map request to command
            var command = new CreateUploadSessionCommand
            {
                Metadata = request.Metadata,
                Signature = request.Signature
            };

            // Send command
            var result = await sender.Send(command);

            // Map result to response
            var response = new PresignedUrlResponse
            {
                UploadUrl = result.UploadUrl,
                UploadId = result.UploadId,
                ExpiresAt = result.ExpiresAt
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized access");
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating pre-signed URL");
            return StatusCode(500, new { error = "Failed to generate pre-signed URL" });
        }
    }

    [HttpPut("/upload/{uploadId}")]
    public async Task<ActionResult<UploadFileResponse>> UploadFile(string uploadId, [FromBody] UpdateDate uploadData)
    {
        logger.LogInformation("Received upload request for upload ID: {UploadId}", uploadId);

        try
        {
            // Read file content
            using var memoryStream = new MemoryStream();
            GetMemoryStreamFromBase64(uploadData.Base64Data).CopyTo(memoryStream);
            var fileData = memoryStream.ToArray();

            // Map to command
            var command = new ProcessUploadCommand
            {
                UploadId = uploadId,
                FileData = fileData
            };

            // Send command
            var result = await sender.Send(command);

            // Map result to response
            var response = new UploadFileResponse
            {
                ImageId = result.ImageId,
                FileName = result.FileName,
                FileSize = result.FileSize,
                UploadedAt = result.UploadedAt
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing upload");
            return StatusCode(500, new { error = "Failed to process upload" });
        }
    }
    private MemoryStream GetMemoryStreamFromBase64(string base64String)
    {
        var fileBytes = Convert.FromBase64String(base64String);
        return new MemoryStream(fileBytes);
    }
}

public class UpdateDate
{
    public required string Base64Data { get; set; }
}