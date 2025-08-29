using Chainly.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Chainly.Data.Constants;

namespace Chainly.Data.Helpers
{
    public static class DBIntializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var roles = Enum.GetNames(typeof(Roles));
                foreach (var role in roles)
                {
                    var roleExist = await roleManager.RoleExistsAsync(role);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new IdentityRole<int>(role));
                    }
                }


                if (!context.Companies.Any())
                {
                    context.Companies.AddRange(
                        new Company
                        {
                            Name = "OpenAI",
                            LocationLatitude = "37.7749",
                            LocationLongitude = "-122.4194",
                            Logo = null
                        },
                        new Company
                        {
                            Name = "Microsoft",
                            LocationLatitude = "47.6062",
                            LocationLongitude = "-122.3321",
                            Logo = null
                        }
                    );
                    await context.SaveChangesAsync();
                }

                var defaultUser = new User
                {
                    FullName = "Admin",
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    CompanyId = 1,
                };

                var user = await userManager.FindByEmailAsync(defaultUser.Email);

                if (user is null)
                {
                    await userManager.CreateAsync(defaultUser, "P@ssword123");
                    await userManager.AddToRolesAsync(defaultUser, new List<string>
                    {
                        Roles.Manager.ToString(),
                    });
                }

                await roleManager.SeedAllPermissions(Roles.Manager.ToString());
            }
        }

        public static async Task SeedAllPermissions(this RoleManager<IdentityRole<int>> roleManager, string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return;

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var allPermissions = Permissions.GenerateAllPermissions();

            foreach (var permission in allPermissions)
            {
                if (!existingClaims.Any(c => c.Type == CustomClaimTypes.Permission && c.Value == permission))
                    await roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
            }
        }
    }
}