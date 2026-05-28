using Microsoft.AspNetCore.Mvc;

namespace MVCCallWebAPI.ViewModels
{
    public class ManagePermissionViewModel
    {
        public string RoleId { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public List<PermissionCheckbox> Permissions { get; set; } = new();
    }
}
