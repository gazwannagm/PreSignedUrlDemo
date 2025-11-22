namespace AppService.Domain.Entities;

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string ImageId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Product()
    {
        Id = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }
}