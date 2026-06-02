using Microsoft.AspNetCore.Identity;
using Project.Models;

namespace Project.Services
{
    public static class IdentitySeedService
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "User" };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string adminEmail = "admin@betmanager.local";
            string adminPassword = "Admin12345!";

            var adminIdentityUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminIdentityUser == null)
            {
                adminIdentityUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminIdentityUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminIdentityUser, "Admin");

                    bool adminProfileExists = context.AppUsers
                        .Any(u => u.IdentityUserId == adminIdentityUser.Id);

                    if (!adminProfileExists)
                    {
                        var adminProfile = new ApplicationUser
                        {
                            IdentityUserId = adminIdentityUser.Id,
                            IdNumber = "0000000000000",
                            FirstName = "System",
                            Surname = "Administrator",
                            Email = adminEmail,
                            PhoneNumber = "0000000000"
                        };

                        context.AppUsers.Add(adminProfile);
                        await context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminIdentityUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminIdentityUser, "Admin");
                }

                bool adminProfileExists = context.AppUsers
                    .Any(u => u.IdentityUserId == adminIdentityUser.Id);

                if (!adminProfileExists)
                {
                    var adminProfile = new ApplicationUser
                    {
                        IdentityUserId = adminIdentityUser.Id,
                        IdNumber = "0000000000000",
                        FirstName = "System",
                        Surname = "Administrator",
                        Email = adminEmail,
                        PhoneNumber = "0000000000"
                    };

                    context.AppUsers.Add(adminProfile);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}