using AppService.Domain.Entities;
using AppService.Domain.Repositories;

namespace AppService.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<string, Product> _products = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<Product?> GetByIdAsync(string id)
    {
        await _lock.WaitAsync();
        try
        {
            return _products.GetValueOrDefault(id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<Product>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _products.Values.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(Product product)
    {
        await _lock.WaitAsync();
        try
        {
            _products[product.Id] = product;
            await Task.CompletedTask;
        }
        finally
        {
            _lock.Release();
        }
    }
}