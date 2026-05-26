using demoWebAPI.Authorization.Requirements;
using demoWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using MVCCallWebAPI.ViewModels;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    private readonly EcomDbContext _context;

    private readonly IAuthorizationService
        _authorizationService;

    private readonly IWebHostEnvironment _env;

    public ProductsController(
    IProductService service,
    EcomDbContext context,
    IAuthorizationService authorizationService,
    IWebHostEnvironment env)
    {
        _service = service;

        _context = context;

        _authorizationService =
            authorizationService;

        _env = env;
    }


    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
    int page = 1,
    int pageSize = 5,
    string? keyword = null,
    int? categoryId = null)
    {
        var query = _context.Products
    .Include(x => x.Category)
    .Include(x => x.Productimages)
    .AsQueryable(); ;

        // FILTER: keyword
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Name.Contains(keyword));
        }


        // FILTER: category
        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        var totalItems = await query.CountAsync();

        var products = await query
    .OrderByDescending(x => x.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(x => new ProductDto
    {
        Id = x.Id,

        Name = x.Name,

        Price = (decimal)x.Price,

        Stock = x.Stock,

        MainImageUrl = x.Productimages
            .Where(i => i.IsMain)
            .Select(i => i.ImageUrl)
            .FirstOrDefault(),

        Category = x.Category == null
            ? null
            : new CategoryDto
            {
                Id = x.Category.Id,
                Name = x.Category.Name
            }
    })
    .ToListAsync();

        var result = new PagedResult<ProductDto>
        {
            Items = products,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    [HttpPost("{id}/images")]
public async Task<IActionResult> UploadProductImages(
    int id,
    [FromForm] List<IFormFile> files)    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { message = "No files uploaded." });
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Product not found." });
        }

        // RESOURCE AUTHORIZATION
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, product, new ProductOwnerRequirement());
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadFolder = Path.Combine(webRoot, "images", "products", id.ToString());
        Directory.CreateDirectory(uploadFolder);

        var createdImages = new List<object>();

        foreach (var file in files)
        {
            if (file.Length <= 0)
                continue;

            // GET FILE EXTENSION
            var ext = Path.GetExtension(file.FileName);

            // ALLOWED EXTENSIONS
            var allowedExtensions =
                new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(ext.ToLower()))
            {
                return BadRequest(new
                {
                    message = "Invalid file type"
                });
            }

            // LIMIT SIZE 5MB
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    message = "File too large"
                });
            }

            var fileName = $"{Guid.NewGuid()}{ext}";

            var fullPath =
                Path.Combine(uploadFolder, fileName);

            using (var stream =
                new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl =
                $"/images/products/{id}/{fileName}";

            var img = new Productimage
            {
                ProductId = id,
                ImageUrl = relativeUrl,
                CreatedDate = DateTime.UtcNow
            };

            _context.Productimages.Add(img);

            createdImages.Add(new
            {
                img.ImageUrl
            });
        }

        // If product has no main image, mark the first uploaded as main
        var hasMain = await _context.Productimages.AnyAsync(pi => pi.ProductId == id && pi.IsMain == true);
        if (!hasMain)
        {
            var first = await _context.Productimages.FirstOrDefaultAsync(pi => pi.ProductId == id);
            if (first != null)
            {
                first.IsMain = true;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Uploaded successfully", images = createdImages });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>>
        GetProduct(int id)
    {
        var product = await _context.Products
        .Include(p => p.Category)
        .Include(p => p.Productimages)
        .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return null;
        }

        return new ProductDto
        {
            Id = product.Id,

            Name = product.Name,

            Price = product.Price,

            Stock = product.Stock,

            MainImageUrl = product.Productimages
                .Where(x => x.IsMain)
                .Select(x => x.ImageUrl)
                .FirstOrDefault(),

            Images = product.Productimages
                .Select(x => x.ImageUrl)
                .ToList(),

            Category = product.Category == null
                ? null
                : new CategoryDto
                {
                    Id = product.Category.Id,
                    Name = product.Category.Name
                }
        };
    }

    [HttpPost]
    //[Authorize(Policy = "Product.Create")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>>
    CreateProduct([FromForm] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created =
            await _service.CreateAsync(dto);

        // LOAD ENTITY
        var product = await _context.Products
            .Include(x => x.Category)
            .Include(x => x.Productimages)
            .FirstOrDefaultAsync(x => x.Id == created.Id);

        if (product == null)
        {
            return NotFound();
        }

        var result = new ProductDto
        {
            Id = product.Id,

            Name = product.Name,

            Price = product.Price,

            Stock = product.Stock,

            MainImageUrl = product.Productimages
                .Where(x => x.IsMain)
                .Select(x => x.ImageUrl)
                .FirstOrDefault(),

            Category = product.Category == null
                ? null
                : new CategoryDto
                {
                    Id = product.Category.Id,
                    Name = product.Category.Name
                }
        };

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = result.Id },
            result
        );
    }
    [HttpPut("{id}")]
    [Authorize(Policy = "Product.Edit")]
    public async Task<IActionResult> UpdateProduct(
    int id,
    [FromForm] UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(x => x.Productimages)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Stock = dto.Stock;

        var uploadFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "images",
            "products",
            id.ToString());

        Directory.CreateDirectory(uploadFolder);

        // DELETE IMAGES
        if (dto.DeletedImages != null)
        {
            foreach (var deleted in dto.DeletedImages)
            {
                var image = product.Productimages
                    .FirstOrDefault(x => x.ImageUrl == deleted);

                if (image != null)
                {
                    var physicalPath =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            deleted.TrimStart('/')
                        );

                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }

                    _context.Productimages.Remove(image);
                }
            }
        }

        // ADD NEW IMAGES
        if (dto.Images != null)
        {
            foreach (var file in dto.Images)
            {
                if (file.Length <= 0)
                    continue;

                var ext =
                    Path.GetExtension(file.FileName);

                var fileName =
                    $"{Guid.NewGuid()}{ext}";

                var fullPath =
                    Path.Combine(uploadFolder, fileName);

                using (var stream =
                    new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl =
                    $"/images/products/{id}/{fileName}";

                product.Productimages.Add(new Productimage
                {
                    ImageUrl = imageUrl,
                    ProductId = id,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        // MAIN IMAGE
        foreach (var img in product.Productimages)
        {
            img.IsMain =
                img.ImageUrl == dto.MainImageUrl;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Updated successfully"
        });
    }
    [HttpDelete("{id}")]
    [Authorize(Policy = "Product.Delete")]
    public async Task<IActionResult>
    DeleteProduct(int id)
    {
        var product = await _context.Products
            .Include(x => x.Productimages)
            .Include(x => x.Orderdetails)
            .Include(x => x.Reviews)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var authorizationResult =
            await _authorizationService
                .AuthorizeAsync(
                    User,
                    product,
                    new ProductOwnerRequirement());

        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        _context.Productimages.RemoveRange(product.Productimages);

        _context.Orderdetails.RemoveRange(product.Orderdetails);

        _context.Reviews.RemoveRange(product.Reviews);

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Deleted successfully"
        });
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<
        ActionResult<IEnumerable<ProductDto>>>
        SearchProducts(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(new
            {
                message = "Keyword is required"
            });
        }

        var result =
            await _service.SearchAsync(keyword);

        return Ok(result);
    }
}