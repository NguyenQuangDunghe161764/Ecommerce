using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.ViewModels
{
    public class ManagePermissionViewModel
    {
        public string RoleId { get; set; }

        public string RoleName { get; set; }

        public List<PermissionCheckbox> Permissions { get; set; }
    }
}
