using AppService.Api.Models.Requests;
using AppService.Api.Models.Responses;
using AppService.Api.Validators;
using AppService.Application.Commands.CreateProduct;
using AppService.Application.Queries.GetAllProducts;
using AppService.Application.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController(
    ISender sender,
    ILogger<ProductsController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        logger.LogInformation("Creating product: {Name} with image ID: {ImageId}", request.Name, request.ImageId);

        // Validate request
        var validator = new CreateProductValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var command = new CreateProductCommand
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageId = request.ImageId
        };

        var result = await sender.Send(command);

        var response = new ProductResponse
        {
            ProductId = result.ProductId,
            Name = result.Name,
            Description = result.Description,
            Price = result.Price,
            ImageId = result.ImageId,
            CreatedAt = result.CreatedAt
        };

        return CreatedAtAction(nameof(GetProduct), new { id = result.ProductId }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(string id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var productDto = await sender.Send(query);

        if (productDto == null)
        {
            return NotFound();
        }

        var response = new ProductResponse
        {
            ProductId = productDto.ProductId,
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            ImageId = productDto.ImageId,
            CreatedAt = productDto.CreatedAt
        };

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAllProducts()
    {
        var command = new GetAllProductsQuery();

        var products = await sender.Send(command);

        var response = products.Select(p => new ProductResponse
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageId = p.ImageId,
            CreatedAt = p.CreatedAt
        }).ToList();

        return Ok(response);
    }
}