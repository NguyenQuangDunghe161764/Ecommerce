using demoWebAPI.Models;
using demoWebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace demoWebAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly EcomDbContext _context;
        private readonly ShippingService _shippingService;
        private readonly ILogger<AddressController> _logger;

        public AddressController(EcomDbContext context, ShippingService shippingService, ILogger<AddressController> logger)
        {
            _context = context;
            _shippingService = shippingService;
            _logger = logger;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token không hợp lệ hoặc thiếu UserId claim");

            var addresses = await _context.UserAddresses
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsDefault)
                .ToListAsync();

            return Ok(addresses);
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Address model)
        {
            _logger.LogInformation("Create address called with model: {@Model}", model);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in JWT claims");
                    return Unauthorized("Không lấy được UserId từ JWT");
                }

                model.UserId = userId;
                model.CreatedDate = DateTime.UtcNow;

                _logger.LogInformation("Processing address for UserId: {UserId}", userId);

                // Nếu user chưa có default => auto set
                var hasDefault = await _context.UserAddresses
                    .AnyAsync(x => x.UserId == userId && x.IsDefault);

                if (!hasDefault)
                {
                    model.IsDefault = true;
                }

                // Nếu set default mới => reset cái cũ
                if (model.IsDefault)
                {
                    var oldDefaults = await _context.UserAddresses
                        .Where(x => x.UserId == userId && x.IsDefault)
                        .ToListAsync();

                    foreach (var item in oldDefaults)
                        item.IsDefault = false;
                }

                _context.UserAddresses.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Address created successfully with ID: {AddressId}", model.Id);
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    message = "Lỗi hệ thống API",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // ================= SET DEFAULT =================
        [HttpPut("{id}/default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var addresses = await _context.UserAddresses
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var selected = addresses.FirstOrDefault(x => x.Id == id);

            if (selected == null)
                return NotFound("Không tìm thấy địa chỉ");

            foreach (var item in addresses)
                item.IsDefault = false;

            selected.IsDefault = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Default address updated" });
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (address == null)
                return NotFound("Không tìm thấy địa chỉ");

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        // ================= CALCULATE SHIPPING =================
        [AllowAnonymous]
        [HttpPost("calculate-ship")]
        public async Task<IActionResult> CalculateShip([FromBody] string customerAddress)
        {
            if (string.IsNullOrWhiteSpace(customerAddress))
                return BadRequest("Địa chỉ không được trống");

            var fee = await _shippingService.CalculateShippingFeeAsync(customerAddress);

            return Ok(new { shippingFee = fee });
        }
    }
}