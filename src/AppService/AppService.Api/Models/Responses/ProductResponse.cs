namespace AppService.Api.Models.Responses;

public class ProductResponse
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string ImageId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}