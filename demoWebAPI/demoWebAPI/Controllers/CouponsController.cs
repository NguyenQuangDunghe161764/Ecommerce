using demoWebAPI.DTOs;
using demoWebAPI.Models;
using demoWebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demoWebAPI.Controllers
{
    [ApiController]
    [Route("api/coupons")]
    public class CouponsController : ControllerBase
    {
        private readonly EcomDbContext _context;
        private readonly ICouponService _couponService;

        public CouponsController(
            EcomDbContext context,
            ICouponService couponService)
        {
            _context = context;
            _couponService = couponService;
        }

        // ================= APPLY / VALIDATE (auth user) =================
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("apply")]
        public async Task<ActionResult<ApplyCouponResultDto>> Apply(
            [FromBody] ApplyCouponDto dto)
        {
            var result = await _couponService.ValidateAsync(
                dto.Code, dto.OrderAmount);

            if (!result.IsValid)
                return Ok(new ApplyCouponResultDto
                {
                    IsValid = false,
                    Message = result.Error,
                    Code = dto.Code,
                    DiscountAmount = 0,
                    FinalAmount = dto.OrderAmount
                });

            return Ok(new ApplyCouponResultDto
            {
                IsValid = true,
                Message = "Áp dụng mã thành công.",
                Code = result.Coupon!.Code,
                DiscountAmount = result.DiscountAmount,
                FinalAmount = dto.OrderAmount - result.DiscountAmount
            });
        }

        // ================= ADMIN: LIST =================
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var coupons = await _context.Coupons
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return Ok(coupons);
        }

        // ================= ADMIN: CREATE =================
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var code = dto.Code.Trim().ToUpperInvariant();

            if (await _context.Coupons.AnyAsync(c => c.Code == code))
                return Conflict(new { message = "Mã giảm giá đã tồn tại." });

            var coupon = new Coupon
            {
                Code = code,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinOrderAmount = dto.MinOrderAmount,
                MaxDiscountAmount = dto.MaxDiscountAmount,
                UsageLimit = dto.UsageLimit,
                UsedCount = 0,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = coupon.Id }, coupon);
        }

        // ================= ADMIN: UPDATE =================
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCouponDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
                return NotFound(new { message = "Không tìm thấy mã giảm giá." });

            coupon.DiscountType = dto.DiscountType;
            coupon.DiscountValue = dto.DiscountValue;
            coupon.MinOrderAmount = dto.MinOrderAmount;
            coupon.MaxDiscountAmount = dto.MaxDiscountAmount;
            coupon.UsageLimit = dto.UsageLimit;
            coupon.StartDate = dto.StartDate;
            coupon.EndDate = dto.EndDate;
            coupon.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(coupon);
        }

        // ================= ADMIN: DELETE =================
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
                return NotFound(new { message = "Không tìm thấy mã giảm giá." });

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa mã giảm giá." });
        }
    }
}
