using System;
using System.Collections.Generic;

namespace demoWebAPI.Models;

public partial class Permission
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public ICollection<RolePermission>
    RolePermissions
    { get; set; }
    = new List<RolePermission>();
}
