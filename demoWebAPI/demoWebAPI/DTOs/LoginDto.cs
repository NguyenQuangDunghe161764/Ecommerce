using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
