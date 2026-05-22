namespace MVCCallWebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
public class ProductViewModel
{
    public int Id { get; set; }
    public List<IFormFile>? Images
    {
        get; set;
    }
    public string? MainImageUrl { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public CategoryViewModel? Category { get; set; }
}