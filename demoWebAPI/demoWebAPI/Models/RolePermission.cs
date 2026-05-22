using Microsoft.AspNetCore.Identity;

namespace demoWebAPI.Models;

public class RolePermission
{
    public string RoleId { get; set; } = null!;

    public int PermissionId { get; set; }

    public IdentityRole Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}