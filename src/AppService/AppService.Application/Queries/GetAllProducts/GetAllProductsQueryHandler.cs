using AppService.Domain.Repositories;
using MediatR;

namespace AppService.Application.Queries.GetAllProducts;

public class GetAllProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync();

        return products.Select(p => new ProductDto
        {
            ProductId = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageId = p.ImageId,
            CreatedAt = p.CreatedAt
        }).ToList();
    }
}