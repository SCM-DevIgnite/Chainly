using Chainly.Data.Constants;
using Chainly.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chainly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Permissions.Users.View)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.FullName
                })
                .ToListAsync();

            var usersWithRoles = new List<object>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(user.Id.ToString()));
                usersWithRoles.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    Roles = roles
                });
            }

            return Ok(usersWithRoles);
        }

        [HttpGet("{id}/roles")]
        [Authorize(Permissions.Users.View)]
        public async Task<IActionResult> GetUserRoles(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            var allRoles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var result = allRoles.Select(r => new
            {
                RoleName = r.Name,
                IsSelected = userRoles.Contains(r.Name)
            });

            return Ok(result);
        }

        [HttpPut("{id}/roles")]
        [Authorize(Permissions.Users.Update)]
        public async Task<IActionResult> UpdateUserRoles(int id, [FromBody] List<string> rolesToAssign)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Except(rolesToAssign);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            var rolesToAdd = rolesToAssign.Except(currentRoles);
            await _userManager.AddToRolesAsync(user, rolesToAdd);

            return NoContent();
        }
    }
}