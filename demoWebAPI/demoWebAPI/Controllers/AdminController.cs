using demoWebAPI.Models;
using demoWebAPI.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demoWebAPI.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EcomDbContext _context;

        public AdminController(
            RoleManager<IdentityRole> roleManager,
                EcomDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // GET: api/admin/roles
        [HttpGet("roles")]
        public IActionResult Roles()
        {
            var roles = _roleManager.Roles
                .Select(r => new
                {
                    r.Id,
                    r.Name
                })
                .ToList();

            return Ok(roles);
        }

        // GET: api/admin/role-permissions/{roleId}
        [HttpGet("role-permissions/{roleId}")]
        public async Task<IActionResult>
            ManageRolePermissions(string roleId)
        {
            var role =
                await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return NotFound();
            }

            var permissions =
                await _context.Permissions.ToListAsync();

            var rolePermissions =
                await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

            var vm = new ManagePermissionViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,

                Permissions = permissions.Select(p =>
                    new PermissionCheckbox
                    {
                        PermissionId = p.Id,
                        PermissionName = p.Name,
                        IsSelected =
                            rolePermissions.Contains(p.Id)
                    }).ToList()
            };

            return Ok(vm);
        }

        // POST: api/admin/role-permissions
        [HttpPost("role-permissions")]
        public async Task<IActionResult>
            UpdateRolePermissions(
                [FromBody]
                ManagePermissionViewModel vm)
        {
            // Validate input
            if (vm == null || string.IsNullOrWhiteSpace(vm.RoleId))
            {
                return BadRequest(new { message = "Invalid payload." });
            }

            if (vm.Permissions == null)
            {
                return BadRequest(new { message = "Permissions list is required." });
            }

            // Ensure role exists
            var role = await _roleManager.FindByIdAsync(vm.RoleId);
            if (role == null)
            {
                return NotFound(new { message = "Role not found." });
            }

            // Use a transaction to ensure consistency
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingPermissions = _context.RolePermissions
                    .Where(rp => rp.RoleId == vm.RoleId);

                _context.RolePermissions.RemoveRange(existingPermissions);

                foreach (var permission in vm.Permissions)
                {
                    if (permission.IsSelected)
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = vm.RoleId,
                            PermissionId = permission.PermissionId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Updated successfully" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Failed to save role permissions.", detail = ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred.", detail = ex.Message });
            }
        }
    }
}