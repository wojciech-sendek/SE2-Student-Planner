using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Data
{
    public static class DbSeeder
    {
        //TODO: seed with actual data
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (dbContext.Database.IsRelational())
            {
                await dbContext.Database.MigrateAsync();
            }

            var roles = new[] { "User", "Manager", "Admin" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (!await dbContext.Faculties.AnyAsync())
            {
                var faculties = new List<Faculty>
                {
                    new() { Name = "university", DisplayName = "University" },
                    new() { Name = "mathematics", DisplayName = "Faculty of Mathematics" },
                    new() { Name = "electronics", DisplayName = "Faculty of Electronics" },
                    new() { Name = "computer-science", DisplayName = "Faculty of Computer Science" }
                };

                dbContext.Faculties.AddRange(faculties);
                await dbContext.SaveChangesAsync();
            }

            var adminEmail = "admin@pw.edu.pl";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, "Admin123!");

                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}