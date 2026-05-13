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
                var retryCount = 0;
                while (retryCount < 5)
                {
                    try
                    {
                        await dbContext.Database.MigrateAsync();
                        break;
                    }
                    catch (Exception)
                    {
                        retryCount++;
                        if (retryCount >= 5) throw;
                        await Task.Delay(5000);
                    }
                }
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

            var managerEmail = "manager.math@pw.edu.pl";
            var managerUser = await userManager.Users
                .Include(u => u.Faculties)
                .FirstOrDefaultAsync(u => u.Email == managerEmail);

            if (managerUser is null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    EmailConfirmed = true,
                    FirstName = "Math",
                    LastName = "Manager"
                };

                var createManagerResult = await userManager.CreateAsync(managerUser, "Manager123!");

                if (createManagerResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }
            else if (!await userManager.IsInRoleAsync(managerUser, "Manager"))
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }

            if (managerUser is not null)
            {
                var universityFaculty = await dbContext.Faculties.FirstAsync(f => f.Name == "university");
                var mathFaculty = await dbContext.Faculties.FirstAsync(f => f.Name == "mathematics");

                if (managerUser.Faculties.All(f => f.Id != universityFaculty.Id))
                {
                    managerUser.Faculties.Add(universityFaculty);
                }

                if (managerUser.Faculties.All(f => f.Id != mathFaculty.Id))
                {
                    managerUser.Faculties.Add(mathFaculty);
                }

                await userManager.UpdateAsync(managerUser);
            }

            if (!await dbContext.AcademicEvents.AnyAsync())
            {
                var universityFaculty = await dbContext.Faculties.FirstAsync(f => f.Name == "university");
                var mathFaculty = await dbContext.Faculties.FirstAsync(f => f.Name == "mathematics");
                var electronicsFaculty = await dbContext.Faculties.FirstAsync(f => f.Name == "electronics");

                dbContext.AcademicEvents.AddRange(
                    new AcademicEvent
                    {
                        Title = "University Orientation",
                        StartTime = DateTime.UtcNow.Date.AddDays(7).AddHours(10),
                        EndTime = DateTime.UtcNow.Date.AddDays(7).AddHours(12),
                        Location = "Main Auditorium",
                        FacultyId = universityFaculty.Id
                    },
                    new AcademicEvent
                    {
                        Title = "Mathematics Seminar",
                        StartTime = DateTime.UtcNow.Date.AddDays(8).AddHours(14),
                        EndTime = DateTime.UtcNow.Date.AddDays(8).AddHours(16),
                        Location = "MATH-201",
                        FacultyId = mathFaculty.Id
                    },
                    new AcademicEvent
                    {
                        Title = "Electronics Lab Briefing",
                        StartTime = DateTime.UtcNow.Date.AddDays(9).AddHours(9),
                        EndTime = DateTime.UtcNow.Date.AddDays(9).AddHours(11),
                        Location = "EL-101",
                        FacultyId = electronicsFaculty.Id
                    });

                await dbContext.SaveChangesAsync();
            }
        }
    }
}