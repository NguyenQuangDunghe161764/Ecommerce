using demoWebAPI.DTOs;
using demoWebAPI.Models;
using demoWebAPI.Models.Enums;
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

    public OrdersApiController(EcomDbContext context)
    {
        _context = context;
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

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            Status = "Pending",
            PaymentStatus = PaymentStatus.Pending,
            Orderdetails = orderDetails
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(new CheckoutResponseDto
        {
            OrderId = order.Id,
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
