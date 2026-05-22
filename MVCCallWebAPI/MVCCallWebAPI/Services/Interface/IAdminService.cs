using Microsoft.AspNetCore.Mvc;
using MVCCallWebAPI.ViewModels;

namespace MVCCallWebAPI.Services.Interface
{
    public interface IAdminService
    {
        Task<List<RoleViewModel>> GetRolesAsync();

        Task<ManagePermissionViewModel>
            GetRolePermissionsAsync(string roleId);

        Task UpdateRolePermissionsAsync(
            ManagePermissionViewModel vm);
    }
}
