using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

public static class DbSeeder
{
    // Old method kept so WebAPI/Program.cs does not break if it still calls the old version.
    public static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await SeedUsersAndRolesAsync(userManager, roleManager);
    }

    // New method used by MVC so it can also seed doctor/patient profiles.
    public static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        await SeedUsersAndRolesAsync(userManager, roleManager);
        await SeedApplicationProfilesAsync(userManager, context);
    }

    private static async Task SeedUsersAndRolesAsync(
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
            // Create the role if it does not exist.
            if (!await roleManager.RoleExistsAsync(u.Role))
            {
                await roleManager.CreateAsync(new IdentityRole(u.Role));
            }

            var existingUser = await userManager.FindByEmailAsync(u.Email);

            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    FullName = u.FullName,
                    Email = u.Email,
                    UserName = u.Email,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(user, u.Password);

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, u.Role);
                }
            }
            else
            {
                // Important: if the user already exists, still make sure it is active and has the correct role.
                existingUser.FullName = u.FullName;
                existingUser.IsActive = true;
                existingUser.EmailConfirmed = true;

                await userManager.UpdateAsync(existingUser);

                if (!await userManager.IsInRoleAsync(existingUser, u.Role))
                {
                    await userManager.AddToRoleAsync(existingUser, u.Role);
                }
            }
        }
    }

    private static async Task SeedApplicationProfilesAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        var doctorUser = await userManager.FindByEmailAsync("doctor@hcars.com");
        var patientUser = await userManager.FindByEmailAsync("patient@hcars.com");

        if (doctorUser != null)
        {
            await SeedDoctorProfileAsync(doctorUser, context);
        }

        if (patientUser != null)
        {
            await SeedPatientProfileAsync(patientUser, context);
        }

        await SeedDoctorScheduleAsync(context);
        await SeedDoctorSpecializationAsync(context);
    }

    private static async Task SeedDoctorProfileAsync(
        ApplicationUser doctorUser,
        ApplicationDbContext context)
    {
        var doctorExists = await context.Doctors
            .AnyAsync(d => d.UserId == doctorUser.Id);

        if (!doctorExists)
        {
            context.Doctors.Add(new Doctor
            {
                UserId = doctorUser.Id,
                LicenseNumber = "DOC-1001",
                Bio = "General doctor profile used for testing doctor MVC pages.",
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedPatientProfileAsync(
        ApplicationUser patientUser,
        ApplicationDbContext context)
    {
        var patientExists = await context.Patients
            .AnyAsync(p => p.UserId == patientUser.Id);

        if (!patientExists)
        {
            context.Patients.Add(new Patient
            {
                UserId = patientUser.Id,
                CPRNumber = "990000001",
                ReferenceNumber = "PAT-1001",
                DateOfBirth = new DateTime(2000, 1, 1),
                BloodType = "O+",
                Address = "Bahrain",
                EmergencyContactName = "Emergency Contact",
                EmergencyContactPhone = "39999999",
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedDoctorScheduleAsync(ApplicationDbContext context)
    {
        var doctor = await context.Doctors
            .FirstOrDefaultAsync(d => d.LicenseNumber == "DOC-1001");

        if (doctor == null)
        {
            return;
        }

        var hasSchedule = await context.DoctorSchedules
            .AnyAsync(s => s.DoctorId == doctor.Id);

        if (!hasSchedule)
        {
            context.DoctorSchedules.AddRange(
                new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = DayOfWeek.Sunday,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(14, 0),
                    SlotDurationMinutes = 30
                },
                new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(14, 0),
                    SlotDurationMinutes = 30
                },
                new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = DayOfWeek.Tuesday,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(14, 0),
                    SlotDurationMinutes = 30
                }
            );

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedDoctorSpecializationAsync(ApplicationDbContext context)
    {
        var doctor = await context.Doctors
            .FirstOrDefaultAsync(d => d.LicenseNumber == "DOC-1001");

        if (doctor == null)
        {
            return;
        }

        var specialization = await context.Specializations
            .FirstOrDefaultAsync(s => s.Name == "General Medicine");

        if (specialization == null)
        {
            specialization = new Specialization
            {
                Name = "General Medicine",
                Description = "General medical consultation"
            };

            context.Specializations.Add(specialization);
            await context.SaveChangesAsync();
        }

        var doctorSpecializationExists = await context.DoctorSpecializations
            .AnyAsync(ds => ds.DoctorId == doctor.Id && ds.SpecializationId == specialization.Id);

        if (!doctorSpecializationExists)
        {
            context.DoctorSpecializations.Add(new DoctorSpecialization
            {
                DoctorId = doctor.Id,
                SpecializationId = specialization.Id
            });

            await context.SaveChangesAsync();
        }
    }
}