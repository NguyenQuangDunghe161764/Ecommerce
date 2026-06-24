using demoWebAPI.DTOs;
using demoWebAPI.Models;
using demoWebAPI.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demoWebAPI.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly EcomDbContext _context;
        private readonly ILogger<AdminOrdersController> _logger;

        // Các trạng thái đơn hàng hợp lệ
        private static readonly string[] AllowedStatuses =
        {
            "Pending", "Confirmed", "Shipping", "Completed", "Cancelled"
        };

        public AdminOrdersController(
            EcomDbContext context,
            ILogger<AdminOrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ================= LIST (filter + paging) =================
        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] string? status,
            [FromQuery] string? paymentStatus,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Orderdetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            if (!string.IsNullOrWhiteSpace(paymentStatus)
                && Enum.TryParse<PaymentStatus>(paymentStatus, true, out var ps))
                query = query.Where(o => o.PaymentStatus == ps);

            var total = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new AdminOrderListItemDto
                {
                    Id = o.Id,
                    CustomerName = o.User != null ? o.User.FullName : null,
                    CustomerEmail = o.User != null ? o.User.Email : null,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus.ToString(),
                    ItemCount = o.Orderdetails.Count
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize),
                items = orders
            });
        }

        // ================= DETAIL =================
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminOrderDetailDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Orderdetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            var dto = new AdminOrderDetailDto
            {
                Id = order.Id,
                CustomerName = order.User?.FullName,
                CustomerEmail = order.User?.Email,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus.ToString(),
                ZaloPayAppTransId = order.ZaloPayAppTransId,
                Items = order.Orderdetails.Select(d => new AdminOrderLineDto
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    LineTotal = d.UnitPrice * d.Quantity
                }).ToList()
            };

            return Ok(dto);
        }

        // ================= UPDATE STATUS =================
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateOrderStatusDto dto)
        {
            if (!AllowedStatuses.Contains(dto.Status))
                return BadRequest(new
                {
                    message = $"Trạng thái không hợp lệ. Hợp lệ: {string.Join(", ", AllowedStatuses)}"
                });

            var order = await _context.Orders
                .Include(o => o.Orderdetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            if (order.Status == "Cancelled")
                return BadRequest(new { message = "Đơn đã hủy, không thể đổi trạng thái." });

            // Hủy đơn => hoàn lại tồn kho nếu trước đó đã trừ (đã thanh toán)
            if (dto.Status == "Cancelled"
                && order.PaymentStatus == PaymentStatus.Paid)
            {
                var productIds = order.Orderdetails.Select(d => d.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var detail in order.Orderdetails)
                {
                    var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
                    if (product != null)
                        product.Stock += detail.Quantity;
                }
            }

            order.Status = dto.Status;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Admin cập nhật đơn {OrderId} sang trạng thái {Status}",
                id, dto.Status);

            return Ok(new
            {
                message = "Cập nhật trạng thái thành công.",
                orderId = order.Id,
                status = order.Status
            });
        }
    }
}
