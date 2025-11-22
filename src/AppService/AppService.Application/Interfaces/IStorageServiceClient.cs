using AppService.Application.Models;

namespace AppService.Application.Interfaces;

public interface IStorageServiceClient
{
    Task<PresignedUrlResponse?> RequestPresignedUrlAsync(FileMetadata metadata, string signature);
    Task<bool> ValidateImageAsync(string imageId);
}