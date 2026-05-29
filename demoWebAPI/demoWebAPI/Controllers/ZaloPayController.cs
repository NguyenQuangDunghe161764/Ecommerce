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
[Route("api/zalopay")]
public class ZaloPayController : ControllerBase
{
    private readonly EcomDbContext _context;
    private readonly IZaloPayService _zaloPayService;
    private readonly ILogger<ZaloPayController> _logger;

    public ZaloPayController(
        EcomDbContext context,
        IZaloPayService zaloPayService,
        ILogger<ZaloPayController> logger)
    {
        _context = context;
        _zaloPayService = zaloPayService;
        _logger = logger;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("create")]
    public async Task<ActionResult<ZaloPayCreateResponseDto>> Create(
        [FromBody] ZaloPayCreateRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var appUser = User.FindFirstValue(ClaimTypes.Email) ?? userId ?? "user";

        var order = await _context.Orders
            .Include(o => o.Orderdetails)
            .FirstOrDefaultAsync(o =>
                o.Id == dto.OrderId &&
                o.UserId == userId);

        if (order == null)
        {
            return NotFound(new { message = "Order not found." });
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return BadRequest(new { message = "Order already paid." });
        }

        var result = await _zaloPayService.CreatePaymentAsync(order, appUser);
        if (!result.Success || string.IsNullOrEmpty(result.OrderUrl))
        {
            return BadRequest(new { message = result.Message ?? "Cannot create ZaloPay order." });
        }

        order.ZaloPayAppTransId = result.AppTransId;
        await _context.SaveChangesAsync();

        return Ok(new ZaloPayCreateResponseDto
        {
            OrderUrl = result.OrderUrl,
            AppTransId = result.AppTransId ?? ""
        });
    }

    [AllowAnonymous]
    [HttpPost("callback")]
    public async Task<IActionResult> Callback(
        [FromForm] string data,
        [FromForm] string mac)
    {
        if (!_zaloPayService.VerifyCallback(data, mac))
        {
            _logger.LogWarning("Invalid ZaloPay callback MAC");
            return BadRequest(new { return_code = -1, return_message = "mac not equal" });
        }

        var callbackData = _zaloPayService.ParseCallbackData(data);
        if (callbackData == null)
        {
            return BadRequest(new { return_code = -1, return_message = "invalid data" });
        }

        var order = await _context.Orders
            .Include(o => o.Orderdetails)
            .FirstOrDefaultAsync(o =>
                o.ZaloPayAppTransId == callbackData.AppTransId);

        if (order == null)
        {
            _logger.LogWarning(
                "Order not found for app_trans_id {AppTransId}",
                callbackData.AppTransId);
            return BadRequest(new { return_code = -1, return_message = "order not found" });
        }

        if (order.PaymentStatus != PaymentStatus.Paid)
        {
            foreach (var detail in order.Orderdetails)
            {
                var product = await _context.Products.FindAsync(detail.ProductId);
                if (product == null)
                {
                    continue;
                }

                if (product.Stock < detail.Quantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock for product {ProductId} on order {OrderId}",
                        product.Id,
                        order.Id);
                    return BadRequest(new { return_code = -1, return_message = "insufficient stock" });
                }

                product.Stock -= detail.Quantity;
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.Status = "Paid";
            await _context.SaveChangesAsync();
        }

        return Ok(new { return_code = 1, return_message = "success" });
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("status/{orderId}")]
    public async Task<IActionResult> GetStatus(int orderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            order.Id,
            PaymentStatus = order.PaymentStatus.ToString(),
            order.Status
        });
    }
}
