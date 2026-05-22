public class ProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? MainImageUrl { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public CategoryDto? Category { get; set; }

    // THÊM
    public List<string> Images { get; set; } = new();
}