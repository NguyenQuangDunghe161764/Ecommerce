using demoWebAPI.DTOs;
using demoWebAPI.Models;
using demoWebAPI.Models.Enums;
using demoWebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace demoWebAPI.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrdersApiController : ControllerBase
{
    private readonly EcomDbContext _context;
    private readonly ICouponService _couponService;
    private readonly IEmailService _emailService;

    public OrdersApiController(
        EcomDbContext context,
        ICouponService couponService,
        IEmailService emailService)
    {
        _context = context;
        _couponService = couponService;
        _emailService = emailService;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponseDto>> Checkout(
        [FromBody] CheckoutDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            return BadRequest(new { message = "Cart is empty." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Distinct().Count())
        {
            return BadRequest(new { message = "Some products were not found." });
        }

        decimal total = 0;
        var orderDetails = new List<Orderdetail>();

        foreach (var item in dto.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            if (item.Quantity <= 0)
            {
                return BadRequest(new { message = "Invalid quantity." });
            }

            if (product.Stock < item.Quantity)
            {
                return BadRequest(new
                {
                    message = $"Product '{product.Name}' is out of stock."
                });
            }

            total += product.Price * item.Quantity;

            orderDetails.Add(new Orderdetail
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        // Áp mã giảm giá (nếu có)
        decimal discount = 0;
        string? appliedCode = null;
        Coupon? appliedCoupon = null;

        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var couponResult = await _couponService.ValidateAsync(
                dto.CouponCode.Trim().ToUpperInvariant(), total);

            if (!couponResult.IsValid)
                return BadRequest(new { message = couponResult.Error });

            discount = couponResult.DiscountAmount;
            appliedCoupon = couponResult.Coupon;
            appliedCode = appliedCoupon!.Code;
        }

        var finalAmount = total - discount;

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = finalAmount,
            Status = "Pending",
            PaymentStatus = PaymentStatus.Pending,
            CouponCode = appliedCode,
            DiscountAmount = discount,
            Orderdetails = orderDetails
        };

        _context.Orders.Add(order);

        // Tăng số lần dùng mã
        if (appliedCoupon != null)
        {
            appliedCoupon.UsedCount += 1;
            _context.Coupons.Update(appliedCoupon);
        }

        await _context.SaveChangesAsync();

        // Gửi email xác nhận (không chặn luồng nếu lỗi)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            await _emailService.SendOrderConfirmationAsync(
                user.Email,
                user.FullName ?? user.UserName ?? "Khách hàng",
                order.Id,
                order.TotalAmount);
        }

        return Ok(new CheckoutResponseDto
        {
            OrderId = order.Id,
            SubTotal = total,
            DiscountAmount = discount,
            CouponCode = appliedCode,
            TotalAmount = order.TotalAmount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetOrder(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            order.Id,
            order.TotalAmount,
            order.Status,
            PaymentStatus = order.PaymentStatus.ToString()
        });
    }
}
