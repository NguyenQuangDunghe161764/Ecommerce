using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace demoWebAPI.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [RegularExpression(@"^(03|05|07|08|09)[0-9]{8}$")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
