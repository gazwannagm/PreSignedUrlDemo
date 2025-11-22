using AppService.Application.Interfaces;
using AppService.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppService.Application.Commands.RequestUploadUrl;

public class RequestUploadUrlCommandHandler(
    IStorageServiceClient storageClient,
    ISignatureService signatureService,
    ILogger<RequestUploadUrlCommandHandler> logger)
    : IRequestHandler<RequestUploadUrlCommand, RequestUploadUrlResult>
{
    public async Task<RequestUploadUrlResult> Handle(RequestUploadUrlCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling upload request for file: {FileName}, Size: {FileSize}, Type: {ContentType}",
            request.FileName, request.FileSize, request.ContentType);

        // Create metadata
        var metadata = new FileMetadata
        {
            FileName = request.FileName,
            FileSize = request.FileSize,
            ContentType = request.ContentType,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresIn = 3600
        };

        // Sign metadata
        var signature = signatureService.Sign(metadata);

        // Request pre-signed URL
        var response = await storageClient.RequestPresignedUrlAsync(metadata, signature);

        if (response == null)
        {
            logger.LogError("Failed to get pre-signed URL from Storage Service");
            throw new InvalidOperationException("Failed to generate upload URL");
        }

        logger.LogInformation("Successfully generated upload URL with upload ID: {UploadId}", response.UploadId);

        return new RequestUploadUrlResult
        {
            UploadUrl = response.UploadUrl,
            UploadId = response.UploadId,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(response.ExpiresAt).DateTime
        };
    }
}