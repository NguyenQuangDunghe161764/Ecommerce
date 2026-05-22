using MVCCallWebAPI.Services.Interface;
using MVCCallWebAPI.ViewModels;

namespace MVCCallWebAPI.Services
{
    public class AdminService : IAdminService
    {
        private readonly IApiService _apiService;

        public AdminService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<RoleViewModel>>
            GetRolesAsync()
        {
            var result =
                await _apiService.GetAsync<List<RoleViewModel>>(
                    "api/admin/roles");

            return result ?? new List<RoleViewModel>();
        }

        public async Task<ManagePermissionViewModel>
            GetRolePermissionsAsync(string roleId)
        {
            var result =
                await _apiService.GetAsync<
                    ManagePermissionViewModel>(
                    $"api/admin/role-permissions/{roleId}");

            return result ?? new ManagePermissionViewModel();
        }

        public async Task UpdateRolePermissionsAsync(
            ManagePermissionViewModel vm)
        {
            var result = await _apiService.PostAsync<object>(
                "api/admin/role-permissions",
                vm);

            if (result == null)
            {
                throw new Exception("Failed to update role permissions.");
            }
        }
    }
}