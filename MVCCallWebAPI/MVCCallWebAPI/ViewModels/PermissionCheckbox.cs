using Microsoft.AspNetCore.Mvc;

namespace MVCCallWebAPI.ViewModels
{
    public class PermissionCheckbox
    {
        public int PermissionId { get; set; }

        public string PermissionName { get; set; }

        public bool IsSelected { get; set; }
    }
}
