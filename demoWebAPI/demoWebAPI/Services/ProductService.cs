using AutoMapper;
using demoWebAPI.Models;
using Microsoft.EntityFrameworkCore;

public class ProductService : IProductService
{
    private readonly IMapper _mapper;
    private readonly EcomDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        EcomDbContext context,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // GET ALL
    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Productimages)
            .ToListAsync();

        return _mapper.Map<List<ProductDto>>(products);
    }

    // GET BY ID
    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Productimages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            _logger.LogWarning("Product not found with id {Id}", id);
            return null;
        }

        return _mapper.Map<ProductDto>(product);
    }

    // CREATE
    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId,
            CreatedDate = DateTime.UtcNow
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        // SAVE IMAGES
        if (dto.Images != null && dto.Images.Any())
        {
            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "products",
                product.Id.ToString()
            );

            Directory.CreateDirectory(uploadFolder);

            bool isFirst = true;

            foreach (var file in dto.Images)
            {
                if (file.Length <= 0)
                    continue;

                var ext = Path.GetExtension(file.FileName);

                var fileName = $"{Guid.NewGuid()}{ext}";

                var fullPath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var image = new Productimage
                {
                    ProductId = product.Id,

                    ImageUrl =
                        $"/uploads/products/{product.Id}/{fileName}",

                    IsMain = isFirst,

                    CreatedDate = DateTime.UtcNow
                };

                _context.Productimages.Add(image);

                isFirst = false;
            }

            await _context.SaveChangesAsync();
        }

        // LOAD FULL DATA
        var createdProduct = await _context.Products
            .Include(x => x.Category)
            .Include(x => x.Productimages)
            .FirstOrDefaultAsync(x => x.Id == product.Id);

        return new ProductDto
        {
            Id = createdProduct.Id,

            Name = createdProduct.Name,

            Price = createdProduct.Price,

            Stock = createdProduct.Stock,

            MainImageUrl = createdProduct.Productimages
                .Where(x => x.IsMain)
                .Select(x => x.ImageUrl)
                .FirstOrDefault(),

            Category = createdProduct.Category == null
                ? null
                : new CategoryDto
                {
                    Id = createdProduct.Category.Id,
                    Name = createdProduct.Category.Name
                }
        };
    }

    // UPDATE
    public async Task UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            _logger.LogError("Product not found {Id}", id);
            throw new Exception("Product not found");
        }

        _mapper.Map(dto, product);

        await _context.SaveChangesAsync();
    }

    // DELETE
    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Orderdetails)
            .Include(p => p.Reviews)
            .Include(p => p.Productimages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            throw new Exception("Product not found");

        _context.Orderdetails.RemoveRange(product.Orderdetails);

        _context.Reviews.RemoveRange(product.Reviews);

        _context.Productimages.RemoveRange(product.Productimages);

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }

    // SEARCH
    public async Task<List<ProductDto>> SearchAsync(string keyword)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Productimages)
            .Where(p => p.Name.Contains(keyword))
            .ToListAsync();

        return _mapper.Map<List<ProductDto>>(products);
    }
}