using demoWebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtService
{
    private readonly IConfiguration _config;

    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly EcomDbContext _context;

    public JwtService(
        IConfiguration config,
        UserManager<ApplicationUser> userManager,
        EcomDbContext context)
    {
        _config = config;

        _userManager = userManager;

        _context = context;
    }

    public async Task<string> GenerateToken(
        ApplicationUser user)
    {
        var roles =
            await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id),

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id),

            new Claim(
                ClaimTypes.Email,
                user.Email ?? ""),

            new Claim(
                ClaimTypes.Name,
                user.FullName ?? ""),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        // ROLE CLAIMS
        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        // LOAD ROLE IDS
        // Map role names to AspNetRole ids
        var roleIds = await _context.UserRoles
    .Where(ur => ur.UserId == user.Id)
    .Select(ur => ur.RoleId)
    .ToListAsync();

        var rolePermissions =
    await _context.RolePermissions
        .Include(rp => rp.Permission)
        .ToListAsync();

        var permissionNames =
            rolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList();


        // ADD PERMISSION CLAIMS
        foreach (var permission in permissionNames)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["Jwt:Key"]!));

        var creds =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],

            audience: _config["Jwt:Audience"],

            claims: claims,

            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(
                    _config["Jwt:ExpireMinutes"]!)),

            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString()
             + Guid.NewGuid().ToString();
    }
}