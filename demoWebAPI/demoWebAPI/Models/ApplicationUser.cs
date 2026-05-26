using Microsoft.AspNetCore.Identity;

namespace demoWebAPI.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public string? Address { get; set; }
    public ICollection<Order> Orders
    { get; set; } = new List<Order>();

    public ICollection<Review> Reviews
    { get; set; } = new List<Review>();

    public ICollection<RefreshToken> RefreshTokens
    { get; set; } = new List<RefreshToken>();

}