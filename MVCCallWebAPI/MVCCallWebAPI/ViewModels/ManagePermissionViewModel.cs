using Microsoft.AspNetCore.Mvc;

namespace MVCCallWebAPI.ViewModels
{
    public class ManagePermissionViewModel
    {
        public string RoleId { get; set; }

        public string RoleName { get; set; }

        public List<PermissionCheckbox> Permissions { get; set; }
    }
}
