using AppService.Application.Interfaces;
using AppService.Domain.Entities;
using AppService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppService.Application.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IStorageServiceClient storageClient,
    ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product: {Name} with image ID: {ImageId}", request.Name, request.ImageId);

        // Validate image exists
        var imageExists = await storageClient.ValidateImageAsync(request.ImageId);

        if (!imageExists)
        {
            logger.LogWarning("Image ID {ImageId} not found", request.ImageId);
            throw new InvalidOperationException("Invalid image ID. Please upload the image first.");
        }

        // Create product
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageId = request.ImageId
        };

        await productRepository.AddAsync(product);

        logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

        return new CreateProductResult
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