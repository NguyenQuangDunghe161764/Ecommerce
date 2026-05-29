using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCCallWebAPI.Models;
using MVCCallWebAPI.Services.Interface;
using MVCCallWebAPI.ViewModels;

namespace MVCCallWebAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IProductService _productService;

        public AdminController(IAdminService adminService, IProductService productService)
        {
            _adminService = adminService;
            _productService = productService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("AdminPanel");
        }

        [HttpGet]
        public async Task<IActionResult> Products(int page = 1)
        {
            try
            {
                var result = await _productService.GetProductsAsync(null, null, page, 20);

                return View(result ?? new PagedResult<MVCCallWebAPI.ViewModels.ProductViewModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(new PagedResult<MVCCallWebAPI.ViewModels.ProductViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Users()
        {
            // Placeholder - implement user management integration with API later
            return View();
        }

        // =========================
        // HIỂN THỊ DANH SÁCH ROLES
        // =========================
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            try
            {
                var roles = await _adminService.GetRolesAsync();

                return View(roles ?? new List<RoleViewModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return View(new List<RoleViewModel>());
            }
        }

        // =========================
        // GET: ROLE PERMISSIONS
        // =========================
        [HttpGet]
        public async Task<IActionResult> ManageRolePermissions(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest();
            }

            try
            {
                var vm =
                    await _adminService
                        .GetRolePermissionsAsync(roleId);

                if (vm == null)
                {
                    return NotFound();
                }

                if (vm.Permissions == null)
                {
                    vm.Permissions = new List<PermissionCheckbox>();
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(Roles));
            }
        }

        // =========================
        // POST: UPDATE PERMISSIONS
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            ManageRolePermissions(
                ManagePermissionViewModel vm)
        {
            if (vm.Permissions == null)
            {
                vm.Permissions = new List<PermissionCheckbox>();
            }

            if (string.IsNullOrWhiteSpace(vm.RoleId))
            {
                TempData["Error"] = "Role is missing. Please open Manage Permissions again.";
                return RedirectToAction(nameof(Roles));
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                await _adminService
                    .UpdateRolePermissionsAsync(vm);

                TempData["Success"] =
                    "Permissions updated successfully.";

                return RedirectToAction(nameof(Roles));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return View(vm);
            }
        }
    }
}