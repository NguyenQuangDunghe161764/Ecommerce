using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.DTOs
{
    public class TokenRequest
    {
        public string RefreshToken { get; set; }
    }
}
