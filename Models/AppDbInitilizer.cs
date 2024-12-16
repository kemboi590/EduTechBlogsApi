using EduTechBlogsApi.Models.Helpers;
using Microsoft.AspNetCore.Identity;

namespace EduTechBlogsApi.Models
{
    public class AppDbInitilizer
    {
        public static async Task SeedRolesDb(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                {
                    if (!await roleManager.RoleExistsAsync(UserRoles.Author))
                    {
                        await roleManager.CreateAsync(new IdentityRole(UserRoles.Author));
                    }
                    if (!await roleManager.RoleExistsAsync(UserRoles.Reader))
                    {
                        await roleManager.CreateAsync(new IdentityRole(UserRoles.Reader));
                    }

                }
            }

        }
    }
}
