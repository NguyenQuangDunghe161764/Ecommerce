using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; }

    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }

}
