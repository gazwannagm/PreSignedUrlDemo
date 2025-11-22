using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageService.Application.Interfaces;
using StorageService.Domain.Entities;
using StorageService.Domain.Repositories;

namespace StorageService.Application.Commands.CreateUploadSession;

public class CreateUploadSessionCommandHandler(
    IUploadSessionRepository sessionRepository,
    ISignatureService signatureService,
    ILogger<CreateUploadSessionCommandHandler> logger,
    IConfiguration configuration)
    : IRequestHandler<CreateUploadSessionCommand, CreateUploadSessionResult>
{

    private readonly string _baseUrl = configuration["StorageServicePublicUrl"] 
                                       ?? Environment.GetEnvironmentVariable("STORAGE_SERVICE_PUBLIC_URL") 
                                       ?? "http://localhost:5001";

    public async Task<CreateUploadSessionResult> Handle(CreateUploadSessionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating upload session for file: {FileName}", request.Metadata.FileName);

        // Verify signature
        if (!signatureService.Verify(request.Metadata, request.Signature))
        {
            logger.LogWarning("Invalid signature for file: {FileName}", request.Metadata.FileName);
            throw new UnauthorizedAccessException("Invalid signature");
        }

        // Check if metadata expired
        if (request.Metadata.IsExpired())
        {
            logger.LogWarning("Expired metadata timestamp for file: {FileName}", request.Metadata.FileName);
            throw new InvalidOperationException("Metadata timestamp expired");
        }

        // Create upload session
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var expiresAt = now + request.Metadata.ExpiresIn;

        var session = new UploadSession
        {
            Metadata = request.Metadata,
            Signature = request.Signature,
            ExpiresAt = expiresAt
        };

        await sessionRepository.AddAsync(session);

        var uploadUrl = $"{_baseUrl}/upload/{session.UploadId}";

        logger.LogInformation("Upload session created: {UploadId}, expires at: {ExpiresAt}", session.UploadId, expiresAt);

        return new CreateUploadSessionResult
        {
            UploadUrl = uploadUrl,
            UploadId = session.UploadId,
            ExpiresAt = expiresAt
        };
    }
}
