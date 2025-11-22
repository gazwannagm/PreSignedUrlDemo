using AppService.Application.Queries.GetAllProducts;
using AppService.Domain.Repositories;
using MediatR;

namespace AppService.Application.Queries.GetProductById;

public class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id);

        if (product == null)
        {
            return null;
        }

        return new ProductDto
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageId = product.ImageId,
            CreatedAt = product.CreatedAt
        };
    }
}
