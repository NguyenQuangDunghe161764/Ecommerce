using System.ComponentModel.DataAnnotations;

namespace MVCCallWebAPI.ViewModels;

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
