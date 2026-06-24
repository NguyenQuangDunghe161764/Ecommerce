using demoWebAPI.Models;
using demoWebAPI.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demoWebAPI.Controllers
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    public class AdminReportsController : ControllerBase
    {
        private readonly EcomDbContext _context;

        public AdminReportsController(EcomDbContext context)
        {
            _context = context;
        }

        // ================= TỔNG QUAN =================
        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
        {
            var paidOrders = _context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Paid);

            var totalRevenue = await paidOrders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            var paidOrderCount = await paidOrders.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Users.CountAsync();

            var ordersByStatus = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                totalRevenue,
                paidOrderCount,
                totalOrders,
                totalProducts,
                totalCustomers,
                ordersByStatus
            });
        }

        // ================= DOANH THU THEO NGÀY =================
        [HttpGet("revenue")]
        public async Task<IActionResult> Revenue(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var start = from ?? DateTime.UtcNow.AddDays(-30);
            var end = to ?? DateTime.UtcNow;

            var data = await _context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Paid
                    && o.OrderDate >= start
                    && o.OrderDate <= end)
                .GroupBy(o => o.OrderDate!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(new
            {
                from = start,
                to = end,
                totalRevenue = data.Sum(d => d.Revenue),
                series = data
            });
        }

        // ================= SẢN PHẨM BÁN CHẠY =================
        [HttpGet("top-products")]
        public async Task<IActionResult> TopProducts([FromQuery] int top = 10)
        {
            if (top < 1 || top > 100) top = 10;

            var data = await _context.Orderdetails
                .Where(d => d.Order.PaymentStatus == PaymentStatus.Paid)
                .GroupBy(d => new { d.ProductId, d.Product.Name })
                .Select(g => new
                {
                    g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(top)
                .ToListAsync();

            return Ok(data);
        }
    }
}
