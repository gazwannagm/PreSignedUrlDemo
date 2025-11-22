using System.Net.Http.Json;
using AppService.Application.Interfaces;
using AppService.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppService.Infrastructure.Repositories;

public class StorageServiceClient(
    HttpClient httpClient,
    ILogger<StorageServiceClient> logger,
    IConfiguration configuration)
    : IStorageServiceClient
{
    private readonly string _storageServiceUrl = configuration["StorageServiceUrl"]
                                                 ?? Environment.GetEnvironmentVariable("STORAGE_SERVICE_URL")
                                                 ?? "http://localhost:5001";

    public async Task<PresignedUrlResponse?> RequestPresignedUrlAsync(FileMetadata metadata, string signature)
    {
        try
        {
            var request = new { metadata, signature };

            logger.LogInformation("Requesting pre-signed URL from Storage Service at {Url}", _storageServiceUrl);

            var response = await httpClient.PostAsJsonAsync($"{_storageServiceUrl}/internal/presigned-url", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogError("Storage Service returned error: {Error}", error);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<PresignedUrlResponse>();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error communicating with Storage Service");
            return null;
        }
    }

    public async Task<bool> ValidateImageAsync(string imageId)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_storageServiceUrl}/internal/images/{imageId}/validate");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating image with Storage Service");
            return false;
        }
    }
}