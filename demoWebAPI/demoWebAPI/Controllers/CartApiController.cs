using demoWebAPI.DTOs;
using demoWebAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace demoWebAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CartApiController : ControllerBase
    {
        private readonly EcomDbContext _context;
        private readonly ILogger<CartApiController> _logger;

        public CartApiController(
            EcomDbContext context,
            ILogger<CartApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string? CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ================= GET CART =================
        [HttpGet]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await GetCartWithItemsAsync(userId);
            return Ok(BuildResponse(cart));
        }

        // ================= ADD ITEM =================
        [HttpPost("items")]
        public async Task<ActionResult<CartResponseDto>> AddItem(
            [FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            var cart = await GetOrCreateCartAsync(userId);

            var existing = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            var newQuantity = (existing?.Quantity ?? 0) + dto.Quantity;

            if (product.Stock < newQuantity)
                return BadRequest(new
                {
                    message = $"Sản phẩm '{product.Name}' chỉ còn {product.Stock} trong kho."
                });

            if (existing != null)
            {
                existing.Quantity = newQuantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            cart.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var refreshed = await GetCartWithItemsAsync(userId);
            return Ok(BuildResponse(refreshed));
        }

        // ================= UPDATE ITEM QUANTITY =================
        [HttpPut("items/{productId}")]
        public async Task<ActionResult<CartResponseDto>> UpdateItem(
            int productId,
            [FromBody] UpdateCartItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await GetCartWithItemsAsync(userId);
            if (cart == null)
                return NotFound(new { message = "Giỏ hàng trống." });

            var item = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (item == null)
                return NotFound(new { message = "Sản phẩm không có trong giỏ." });

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null && product.Stock < dto.Quantity)
                return BadRequest(new
                {
                    message = $"Sản phẩm '{product.Name}' chỉ còn {product.Stock} trong kho."
                });

            item.Quantity = dto.Quantity;
            cart.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var refreshed = await GetCartWithItemsAsync(userId);
            return Ok(BuildResponse(refreshed));
        }

        // ================= REMOVE ITEM =================
        [HttpDelete("items/{productId}")]
        public async Task<ActionResult<CartResponseDto>> RemoveItem(int productId)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await GetCartWithItemsAsync(userId);
            if (cart == null)
                return NotFound(new { message = "Giỏ hàng trống." });

            var item = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (item == null)
                return NotFound(new { message = "Sản phẩm không có trong giỏ." });

            _context.CartItems.Remove(item);
            cart.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var refreshed = await GetCartWithItemsAsync(userId);
            return Ok(BuildResponse(refreshed));
        }

        // ================= CLEAR CART =================
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await GetCartWithItemsAsync(userId);
            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Đã xóa toàn bộ giỏ hàng." });
        }

        // ================= HELPERS =================
        private async Task<Cart?> GetCartWithItemsAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Productimages)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await GetCartWithItemsAsync(userId);
            if (cart != null)
                return cart;

            cart = new Cart
            {
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        private static CartResponseDto BuildResponse(Cart? cart)
        {
            var response = new CartResponseDto();
            if (cart == null)
                return response;

            foreach (var ci in cart.CartItems.OrderBy(x => x.Id))
            {
                var product = ci.Product;
                var imageUrl = product?.Productimages
                    ?.OrderByDescending(img => img.IsMain)
                    .FirstOrDefault()?.ImageUrl;

                var unitPrice = product?.Price ?? 0;

                response.Items.Add(new CartItemResponseDto
                {
                    ProductId = ci.ProductId,
                    ProductName = product?.Name ?? string.Empty,
                    ImageUrl = imageUrl,
                    UnitPrice = unitPrice,
                    Quantity = ci.Quantity,
                    Stock = product?.Stock ?? 0,
                    LineTotal = unitPrice * ci.Quantity
                });
            }

            response.TotalQuantity = response.Items.Sum(i => i.Quantity);
            response.TotalAmount = response.Items.Sum(i => i.LineTotal);
            return response;
        }
    }
}
