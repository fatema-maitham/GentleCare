using Microsoft.AspNetCore.Identity;
using WebAPI.Models;

public static class DbSeeder
{
    public static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var users = new[]
        {
            new { FullName = "Clinic Manager", Email = "manager@hcars.com", Password = "Manager@123", Role = "ClinicManager" },
            new { FullName = "Doctor User", Email = "doctor@hcars.com", Password = "Doctor@123", Role = "Doctor" },
            new { FullName = "Receptionist User", Email = "receptionist@hcars.com", Password = "Recept@123", Role = "Receptionist" },
            new { FullName = "Patient User", Email = "patient@hcars.com", Password = "Patient@123", Role = "Patient" }
        };

        foreach (var u in users)
        {
           
            if (!await roleManager.RoleExistsAsync(u.Role))
                await roleManager.CreateAsync(new IdentityRole(u.Role));

          
            var existingUser = await userManager.FindByEmailAsync(u.Email);
            if (existingUser != null) continue;

            var user = new ApplicationUser
            {
                FullName = u.FullName,
                Email = u.Email,
                UserName = u.Email,
                IsActive = true,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, u.Password);
            await userManager.AddToRoleAsync(user, u.Role);
        }
    }
}