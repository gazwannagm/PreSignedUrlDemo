using MediatR;
using Microsoft.Extensions.Logging;
using StorageService.Domain.Entities;
using StorageService.Domain.Repositories;

namespace StorageService.Application.Commands.ProcessUpload;

public class ProcessUploadCommandHandler(
    IUploadSessionRepository sessionRepository,
    IImageRepository imageRepository,
    ILogger<ProcessUploadCommandHandler> logger)
    : IRequestHandler<ProcessUploadCommand, ProcessUploadResult>
{
    public async Task<ProcessUploadResult> Handle(ProcessUploadCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing upload for session: {UploadId}", request.UploadId);

        // Get upload session
        var session = await sessionRepository.GetByIdAsync(request.UploadId);
        if (session == null)
        {
            logger.LogWarning("Upload session not found: {UploadId}", request.UploadId);
            throw new InvalidOperationException("Invalid or expired upload ID");
        }

        // Check expiration
        if (session.IsExpired())
        {
            logger.LogWarning("Upload session expired: {UploadId}", request.UploadId);
            await sessionRepository.RemoveAsync(request.UploadId);
            throw new InvalidOperationException("Upload URL has expired");
        }

        // Verify file size
        if (!session.Metadata.ValidateFileSize(request.FileData.Length))
        {
            logger.LogWarning("File size mismatch. Expected: {Expected}, Got: {Actual}",
                session.Metadata.FileSize, request.FileData.Length);
            throw new InvalidOperationException($"File size mismatch. Expected {session.Metadata.FileSize} bytes, got {request.FileData.Length} bytes");
        }

        // Store the image
        var image = new StoredImage
        {
            FileName = session.Metadata.FileName,
            ContentType = session.Metadata.ContentType,
            FileSize = request.FileData.Length,
            Data = request.FileData
        };

        await imageRepository.AddAsync(image);
        await sessionRepository.RemoveAsync(request.UploadId);

        logger.LogInformation("Image stored successfully: {ImageId}, File: {FileName}", image.ImageId, image.FileName);

        // Background processing simulation (ASYNC PROCESSING)
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            logger.LogInformation("Background processing completed for image: {ImageId}", image.ImageId);
        });

        return new ProcessUploadResult
        {
            ImageId = image.ImageId,
            FileName = image.FileName,
            FileSize = image.FileSize,
            UploadedAt = image.UploadedAt
        };
    }
}