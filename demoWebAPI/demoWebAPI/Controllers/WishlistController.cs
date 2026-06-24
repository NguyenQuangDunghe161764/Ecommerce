using demoWebAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace demoWebAPI.Controllers
{
    [ApiController]
    [Route("api/wishlist")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WishlistController : ControllerBase
    {
        private readonly EcomDbContext _context;

        public WishlistController(EcomDbContext context)
        {
            _context = context;
        }

        private string? CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ================= GET WISHLIST =================
        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var items = await _context.WishlistItems
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                    .ThenInclude(p => p.Productimages)
                .OrderByDescending(w => w.CreatedDate)
                .Select(w => new
                {
                    w.ProductId,
                    ProductName = w.Product.Name,
                    w.Product.Price,
                    w.Product.Stock,
                    MainImageUrl = w.Product.Productimages
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    w.CreatedDate
                })
                .ToListAsync();

            return Ok(items);
        }

        // ================= ADD =================
        [HttpPost("{productId}")]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            var exists = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

            if (exists)
                return Ok(new { message = "Sản phẩm đã có trong danh sách yêu thích." });

            _context.WishlistItems.Add(new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã thêm vào yêu thích." });
        }

        // ================= REMOVE =================
        [HttpDelete("{productId}")]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w =>
                    w.UserId == userId && w.ProductId == productId);

            if (item == null)
                return NotFound(new { message = "Sản phẩm không có trong yêu thích." });

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa khỏi yêu thích." });
        }
    }
}
