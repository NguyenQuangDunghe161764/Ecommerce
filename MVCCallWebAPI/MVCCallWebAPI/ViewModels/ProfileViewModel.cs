using System.ComponentModel.DataAnnotations;

namespace MVCCallWebAPI.ViewModels;

public class ProfileViewModel
{
    [Required]
    public string FullName { get; set; }

    public string Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
}