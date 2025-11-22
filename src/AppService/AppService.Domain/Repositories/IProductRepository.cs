using AppService.Domain.Entities;

namespace AppService.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id);
    Task<List<Product>> GetAllAsync();
    Task AddAsync(Product product);
}