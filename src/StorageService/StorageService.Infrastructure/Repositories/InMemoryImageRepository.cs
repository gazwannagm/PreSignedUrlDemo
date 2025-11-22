using StorageService.Domain.Entities;
using StorageService.Domain.Repositories;

namespace StorageService.Infrastructure.Repositories;

public class InMemoryImageRepository : IImageRepository
{
    private readonly Dictionary<string, StoredImage> _images = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<StoredImage?> GetByIdAsync(string imageId)
    {
        await _lock.WaitAsync();
        try
        {
            return _images.GetValueOrDefault(imageId);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(StoredImage image)
    {
        await _lock.WaitAsync();
        try
        {
            _images[image.ImageId] = image;
            await Task.CompletedTask;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> ExistsAsync(string imageId)
    {
        await _lock.WaitAsync();
        try
        {
            return _images.ContainsKey(imageId);
        }
        finally
        {
            _lock.Release();
        }
    }
}