using demoWebAPI.DTOs;
using demoWebAPI.Models;
using demoWebAPI.Models.Enums;
using demoWebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace demoWebAPI.Controllers
{
    [ApiController]
    [Route("api/vnpay")]
    public class VnPayController : ControllerBase
    {
        private readonly EcomDbContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly ILogger<VnPayController> _logger;

        public VnPayController(
            EcomDbContext context,
            IVnPayService vnPayService,
            ILogger<VnPayController> logger)
        {
            _context = context;
            _vnPayService = vnPayService;
            _logger = logger;
        }

        // ================= TẠO URL THANH TOÁN =================
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ZaloPayCreateRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId);

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            if (order.PaymentStatus == PaymentStatus.Paid)
                return BadRequest(new { message = "Đơn hàng đã được thanh toán." });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var paymentUrl = _vnPayService.CreatePaymentUrl(order, ip);

            return Ok(new { paymentUrl });
        }

        // ================= VNPAY REDIRECT VỀ =================
        [AllowAnonymous]
        [HttpGet("return")]
        public async Task<IActionResult> Return()
        {
            var result = _vnPayService.ProcessReturn(Request.Query);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "VNPay return thất bại cho đơn {OrderId}: {Code} - {Msg}",
                    result.OrderId, result.ResponseCode, result.Message);

                return Ok(new
                {
                    success = false,
                    orderId = result.OrderId,
                    responseCode = result.ResponseCode,
                    message = result.Message
                });
            }

            var order = await _context.Orders
                .Include(o => o.Orderdetails)
                .FirstOrDefaultAsync(o => o.Id == result.OrderId);

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            // Chỉ cập nhật + trừ kho 1 lần
            if (order.PaymentStatus != PaymentStatus.Paid)
            {
                foreach (var detail in order.Orderdetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null) continue;

                    if (product.Stock < detail.Quantity)
                    {
                        _logger.LogWarning(
                            "Không đủ kho cho sản phẩm {ProductId} đơn {OrderId}",
                            product.Id, order.Id);
                        return BadRequest(new { message = "Sản phẩm không đủ tồn kho." });
                    }

                    product.Stock -= detail.Quantity;
                }

                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = "Paid";
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                orderId = order.Id,
                message = "Thanh toán VNPay thành công."
            });
        }
    }
}
