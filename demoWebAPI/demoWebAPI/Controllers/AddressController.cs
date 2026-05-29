using demoWebAPI.Models;
using demoWebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public AddressController(EcomDbContext context, ShippingService shippingService)
        {
            _context = context;
            _shippingService = shippingService;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return StatusCode(500, "Lỗi API: Không thể lấy được UserId từ Token JWT. Hãy kiểm tra lại Claim lúc tạo Token ở hàm Login!");
                }

                model.UserId = userId;
                model.CreatedDate = DateTime.UtcNow;

                var hasDefault = await _context.UserAddresses
                    .AnyAsync(x => x.UserId == userId && x.IsDefault);

                if (!hasDefault)
                {
                    model.IsDefault = true;
                }

                if (model.IsDefault)
                {
                    var oldDefaults = await _context.UserAddresses
                        .Where(x => x.UserId == userId && x.IsDefault)
                        .ToListAsync();

                    foreach (var item in oldDefaults)
                    {
                        item.IsDefault = false;
                    }
                }

                _context.UserAddresses.Add(model);
                await _context.SaveChangesAsync();

                return Ok(model);
            }
            catch (Exception ex)
            {
                var actualErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return StatusCode(500, $"Lỗi hệ thống Web API: {actualErrorMessage}");
            }
        }
        // ================= SET DEFAULT =================
        [HttpPut("{id}/default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var addresses = await _context.UserAddresses
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var item in addresses)
            {
                item.IsDefault = false;
            }

            var selected = addresses.FirstOrDefault(x => x.Id == id);
            if (selected == null)
            {
                return NotFound();
            }

            selected.IsDefault = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Default address updated" });
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (address == null)
            {
                return NotFound();
            }

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        // ================= TÍNH PHÍ SHIP (GOONG MAPS) =================
        [AllowAnonymous] 
        [HttpPost("calculate-ship")]
        public async Task<IActionResult> CalculateShip([FromBody] string customerAddress)
        {
            if (string.IsNullOrEmpty(customerAddress))
            {
                return BadRequest("Địa chỉ khách hàng không được trống.");
            }

            var fee = await _shippingService.CalculateShippingFeeAsync(customerAddress);

            return Ok(new { shippingFee = fee });
        }
    }
}