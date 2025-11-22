namespace AppService.Api.Models.Requests;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string ImageId { get; set; } = string.Empty;
}