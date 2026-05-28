using System.ComponentModel.DataAnnotations;

namespace MVCCallWebAPI.ViewModels;

public class ProfileViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    // UI-only fields for cascading address selection
    public string? ProvinceName { get; set; }
    public string? DistrictName { get; set; }
    public string? WardName { get; set; }
    public string? AddressDetail { get; set; }
}