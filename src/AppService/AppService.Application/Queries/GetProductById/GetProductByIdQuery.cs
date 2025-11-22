using AppService.Application.Queries.GetAllProducts;
using MediatR;

namespace AppService.Application.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<ProductDto?>
{
    public string Id { get; set; } = string.Empty;
}
