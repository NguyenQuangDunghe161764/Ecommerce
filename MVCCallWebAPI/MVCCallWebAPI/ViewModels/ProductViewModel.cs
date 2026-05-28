using Microsoft.AspNetCore.Http;

namespace MVCCallWebAPI.ViewModels;

public class ProductViewModel
{
    public int Id { get; set; }

    public string Name { get; set; }
    public int CategoryId { get; set; }
    public decimal Price { get; set; }

    public int Stock { get; set; }
    public string? Description { get; set; }

    public string? MainImageUrl { get; set; }

    public List<string> ExistingImages { get; set; } = new();

    public List<IFormFile> Images { get; set; } = new();

    public List<string> DeletedImages { get; set; } = new();

    public List<string> ImageOrders { get; set; } = new();

    public CategoryViewModel? Category { get; set; }
}