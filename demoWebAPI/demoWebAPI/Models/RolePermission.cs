using Microsoft.AspNetCore.Identity;

namespace demoWebAPI.Models;

public class RolePermission
{
    public string RoleId { get; set; }

    public int PermissionId { get; set; }

    public IdentityRole Role { get; set; }

    public Permission Permission { get; set; }
}