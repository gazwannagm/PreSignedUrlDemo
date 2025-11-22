using StorageService.Domain.Entities;

namespace StorageService.Domain.Repositories;

public interface IImageRepository
{
    Task<StoredImage?> GetByIdAsync(string imageId);
    Task AddAsync(StoredImage image);
    Task<bool> ExistsAsync(string imageId);
}