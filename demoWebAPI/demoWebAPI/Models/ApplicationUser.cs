using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public ICollection<Order> Orders { get; set; }
    = new List<Order>();

        public ICollection<Review> Reviews { get; set; }
            = new List<Review>();

        public ICollection<RefreshToken> RefreshTokens
        { get; set; }
            = new List<RefreshToken>();

    }
}
