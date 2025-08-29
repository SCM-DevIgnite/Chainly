using Chainly.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.Helpers
{
    public static class DBIntializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();


                string[] roles = { "Manager", "Employee" };
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
            }
        }
    }
}
