using AutoMapper;
using demoWebAPI.Models;
using Microsoft.EntityFrameworkCore;

public class ProductService : IProductService
{
    private readonly IMapper _mapper;
    private readonly EcomDbContext _context;

    private readonly ILogger<ProductService> _logger;

    public ProductService(EcomDbContext context, IMapper mapper, ILogger<ProductService> logger)
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
            .ToListAsync(); return _mapper.Map<List<ProductDto>>(products);
    }

    // GET BY ID
    public async Task<ProductDto> GetByIdAsync(int id)
    {
        _logger.LogWarning("Product not found with id {Id}", id);
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return null;

        return _mapper.Map<ProductDto>(product);
    }

    // CREATE
    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        _logger.LogInformation("Creating product: {Name}", dto.Name);
        var product = _mapper.Map<Product>(dto);

        product.CreatedDate = DateTime.UtcNow;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);

    }

    // UPDATE
    public async Task UpdateAsync(int id, UpdateProductDto dto)
    {
        _logger.LogError("Error while updating product {Id}", id);
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            throw new Exception("Product not found");

        _mapper.Map(dto, product);

        await _context.SaveChangesAsync();
    }

    // DELETE
    public async Task DeleteAsync(int id)
    {
        _logger.LogWarning("Deleting product Id: {Id}", id);
        var product = await _context.Products
     .Include(p => p.Orderdetails)
     .Include(p => p.Reviews)
     .FirstOrDefaultAsync(p => p.Id == id);

        _context.Orderdetails.RemoveRange(product.Orderdetails);
        _context.Reviews.RemoveRange(product.Reviews);

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }

    //SEARCH
    public async Task<List<ProductDto>> SearchAsync(string keyword)
    {
        var products = await _context.Products
    .Include(p => p.Category)
    .Where(p => p.Name.Contains(keyword))
    .ToListAsync();
        return _mapper.Map<List<ProductDto>>(products);
    }
}