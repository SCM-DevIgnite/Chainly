using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Chainly.Data.Constants;
using Chainly.Data.Models;

namespace Chainly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public RolesController(RoleManager<IdentityRole<int>> roleManager)
        {
            _roleManager = roleManager;
        }


        [HttpGet]
        [Authorize(Permissions.Roles.View)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles.Select(r => new { r.Id, r.Name }));
        }


        [HttpPost]
        [Authorize(Permissions.Roles.Create)]
        public async Task<IActionResult> AddRole([FromBody] IdentityRole<int> role)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
                return BadRequest(new { message = "Role name is required" });

            if (await _roleManager.RoleExistsAsync(role.Name))
                return Conflict(new { message = "Role already exists" });

            var result = await _roleManager.CreateAsync(new IdentityRole<int>(role.Name.Trim()));
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Role created successfully" });
        }


        [HttpGet("{roleId}/permissions")]
        [Authorize(Permissions.Roles.View)]
        public async Task<IActionResult> GetPermissions(int roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null) return NotFound();

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var modules = Enum.GetNames(typeof(Modules));
            var allPermissions = modules.SelectMany(m => Permissions.GeneratePermissionsList(m)).ToList();

            var permissions = allPermissions.Select(p => new
            {
                Name = p,
                IsAssigned = roleClaims.Any(c => c.Type == "Permission" && c.Value == p)
            });

            return Ok(new
            {
                role.Id,
                role.Name,
                Permissions = permissions
            });
        }


        [HttpPut("{roleId}/permissions")]
        [Authorize(Permissions.Roles.Update)]
        public async Task<IActionResult> UpdatePermissions(int roleId, [FromBody] string[] selectedPermissions)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null) return NotFound();

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims)
                await _roleManager.RemoveClaimAsync(role, claim);

            foreach (var permission in selectedPermissions)
                await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));

            return Ok(new { message = "Permissions updated successfully" });
        }
    }
}
