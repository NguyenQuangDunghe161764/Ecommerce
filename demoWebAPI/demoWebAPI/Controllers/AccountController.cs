using demoWebAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly SignInManager<ApplicationUser> _signInManager;

    private readonly EcomDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        EcomDbContext context)
    {
        _userManager = userManager;

        _signInManager = signInManager;

        _context = context;
    }

    // ================= PROFILE =================

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Address
        });
    }

    // ================= UPDATE PROFILE =================

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileDto dto)
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        user.FullName = dto.FullName;

        user.PhoneNumber = dto.PhoneNumber;

        user.Address = dto.Address;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile updated successfully"
        });
    }

    // ================= CHANGE PASSWORD =================

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordDto dto)
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        var result =
            await _userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new
        {
            message = "Password changed successfully"
        });
    }

    // ================= LOGOUT =================

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return Ok(new
        {
            message = "Logged out successfully"
        });
    }
}