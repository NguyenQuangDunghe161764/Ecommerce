using System.ComponentModel.DataAnnotations;

public class UpdateProfileDto
{
    [Required]
    public string FullName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
}