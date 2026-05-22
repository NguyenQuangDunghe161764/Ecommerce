using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.ViewModels
{
    public class PermissionCheckbox
    {
        public int PermissionId { get; set; }

        public string PermissionName { get; set; }

        public bool IsSelected { get; set; }
    }
}
