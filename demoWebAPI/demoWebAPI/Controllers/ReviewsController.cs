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
    public class ReviewsController : ControllerBase
    {
        private readonly EcomDbContext _context;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(
            EcomDbContext context,
            ILogger<ReviewsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string? CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ================= LIST + SUMMARY (public) =================
        [AllowAnonymous]
        [HttpGet("api/products/{productId}/reviews")]
        public async Task<ActionResult<ReviewSummaryDto>> GetReviews(int productId)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            var summary = new ReviewSummaryDto
            {
                ProductId = productId,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Count > 0
                    ? Math.Round(reviews.Average(r => r.Rating), 1)
                    : 0,
                RatingBreakdown = Enumerable.Range(1, 5)
                    .ToDictionary(
                        star => star,
                        star => reviews.Count(r => r.Rating == star)),
                Reviews = reviews.Select(ToDto).ToList()
            };

            return Ok(summary);
        }

        // ================= CREATE =================
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("api/products/{productId}/reviews")]
        public async Task<ActionResult<ReviewResponseDto>> CreateReview(
            int productId,
            [FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            // Mỗi user chỉ đánh giá 1 lần / sản phẩm
            var already = await _context.Reviews
                .AnyAsync(r => r.ProductId == productId && r.UserId == userId);

            if (already)
                return Conflict(new
                {
                    message = "Bạn đã đánh giá sản phẩm này rồi. Hãy dùng chức năng sửa đánh giá."
                });

            // Chỉ cho đánh giá khi đã từng mua sản phẩm
            var hasPurchased = await _context.Orderdetails
                .AnyAsync(od => od.ProductId == productId
                    && od.Order.UserId == userId);

            if (!hasPurchased)
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Bạn cần mua sản phẩm trước khi đánh giá."
                });

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedDate = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            await _context.Entry(review).Reference(r => r.User).LoadAsync();

            return CreatedAtAction(
                nameof(GetReviews),
                new { productId },
                ToDto(review));
        }

        // ================= UPDATE (chủ sở hữu) =================
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("api/reviews/{id}")]
        public async Task<ActionResult<ReviewResponseDto>> UpdateReview(
            int id,
            [FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound(new { message = "Không tìm thấy đánh giá." });

            if (review.UserId != userId)
                return Forbid();

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            await _context.SaveChangesAsync();

            return Ok(ToDto(review));
        }

        // ================= DELETE (chủ sở hữu hoặc Admin) =================
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("api/reviews/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound(new { message = "Không tìm thấy đánh giá." });

            if (review.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa đánh giá." });
        }

        // ================= HELPER =================
        private static ReviewResponseDto ToDto(Review r) => new()
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserId = r.UserId,
            UserName = r.User?.FullName ?? r.User?.UserName,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedDate = r.CreatedDate
        };
    }
}
