using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

public static class DbSeeder
{
    // Keeps old WebAPI calls working if WebAPI/Program.cs still uses the old method.
    public static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await SeedUsersAndRolesAsync(userManager, roleManager);
    }

    // MVC uses this method so users + linked Doctor/Patient profiles + demo data are created.
    public static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        await SeedUsersAndRolesAsync(userManager, roleManager);
        await SeedApplicationDataAsync(userManager, context);
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

    private static async Task SeedApplicationDataAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        await EnsureLookupDataAsync(context);

        var doctorUser = await userManager.FindByEmailAsync("doctor@hcars.com");
        var patientUser = await userManager.FindByEmailAsync("patient@hcars.com");

        if (doctorUser == null || patientUser == null)
        {
            return;
        }

        var doctor = await EnsureDoctorProfileAsync(doctorUser, context);
        var patient = await EnsurePatientProfileAsync(patientUser, context);

        await EnsureDoctorSpecializationAsync(doctor, context);
        await EnsureDoctorScheduleAsync(doctor, context);
        await EnsureDemoAppointmentsAsync(doctor, patient, context);
        await EnsureDoctorNotificationsAsync(doctorUser, context);
    }

    private static async Task EnsureLookupDataAsync(ApplicationDbContext context)
    {
        var statusData = new[]
        {
            new { Name = "Requested", Description = "Appointment has been requested" },
            new { Name = "Confirmed", Description = "Appointment has been confirmed" },
            new { Name = "CheckedIn", Description = "Patient has checked in" },
            new { Name = "InProgress", Description = "Appointment is in progress" },
            new { Name = "Completed", Description = "Appointment has been completed" },
            new { Name = "Cancelled", Description = "Appointment has been cancelled" },
            new { Name = "Missed", Description = "Patient missed the appointment" }
        };

        foreach (var status in statusData)
        {
            var exists = await context.AppointmentStatuses.AnyAsync(s => s.Name == status.Name);

            if (!exists)
            {
                context.AppointmentStatuses.Add(new AppointmentStatusLookup
                {
                    Name = status.Name,
                    Description = status.Description
                });
            }
        }

        var notificationTypes = new[] { "Appointment", "Prescription", "General" };

        foreach (var type in notificationTypes)
        {
            var exists = await context.NotificationTypes.AnyAsync(t => t.Name == type);

            if (!exists)
            {
                context.NotificationTypes.Add(new NotificationType
                {
                    Name = type
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Doctor> EnsureDoctorProfileAsync(
        ApplicationUser doctorUser,
        ApplicationDbContext context)
    {
        var doctor = await context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == doctorUser.Id);

        if (doctor != null)
        {
            doctor.LicenseNumber = "DOC-1001";
            doctor.Bio = "General doctor profile used for testing doctor MVC pages.";
            doctor.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return doctor;
        }

        doctor = new Doctor
        {
            UserId = doctorUser.Id,
            LicenseNumber = "DOC-1001",
            Bio = "General doctor profile used for testing doctor MVC pages.",
            CreatedAt = DateTime.UtcNow
        };

        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();

        return doctor;
    }

    private static async Task<Patient> EnsurePatientProfileAsync(
        ApplicationUser patientUser,
        ApplicationDbContext context)
    {
        var patient = await context.Patients
            .FirstOrDefaultAsync(p => p.UserId == patientUser.Id);

        if (patient != null)
        {
            patient.CPRNumber = "990000001";
            patient.ReferenceNumber = "PAT-1001";
            patient.BloodType = "O+";
            patient.Address = "Bahrain";
            patient.EmergencyContactName = "Emergency Contact";
            patient.EmergencyContactPhone = "39999999";
            patient.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return patient;
        }

        patient = new Patient
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
        };

        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        return patient;
    }

    private static async Task EnsureDoctorSpecializationAsync(
        Doctor doctor,
        ApplicationDbContext context)
    {
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

        var exists = await context.DoctorSpecializations
            .AnyAsync(ds => ds.DoctorId == doctor.Id && ds.SpecializationId == specialization.Id);

        if (!exists)
        {
            context.DoctorSpecializations.Add(new DoctorSpecialization
            {
                DoctorId = doctor.Id,
                SpecializationId = specialization.Id
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureDoctorScheduleAsync(
        Doctor doctor,
        ApplicationDbContext context)
    {
        var hasSchedule = await context.DoctorSchedules
            .AnyAsync(s => s.DoctorId == doctor.Id);

        if (hasSchedule)
        {
            return;
        }

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

    private static async Task EnsureDemoAppointmentsAsync(
        Doctor doctor,
        Patient patient,
        ApplicationDbContext context)
    {
        var confirmedStatus = await context.AppointmentStatuses.FirstAsync(s => s.Name == "Confirmed");
        var checkedInStatus = await context.AppointmentStatuses.FirstAsync(s => s.Name == "CheckedIn");
        var completedStatus = await context.AppointmentStatuses.FirstAsync(s => s.Name == "Completed");

        var today = DateTime.Today;
        var tomorrow = DateTime.Today.AddDays(1);
        var yesterday = DateTime.Today.AddDays(-1);

        await EnsureAppointmentAsync(
            doctor,
            patient,
            confirmedStatus,
            today,
            new TimeOnly(9, 0),
            new TimeOnly(9, 30),
            "Demo confirmed appointment for doctor testing.",
            context);

        await EnsureAppointmentAsync(
            doctor,
            patient,
            checkedInStatus,
            today,
            new TimeOnly(10, 0),
            new TimeOnly(10, 30),
            "Demo checked-in appointment for status workflow testing.",
            context);

        await EnsureAppointmentAsync(
            doctor,
            patient,
            confirmedStatus,
            tomorrow,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30),
            "Demo upcoming appointment.",
            context);

        var completedAppointment = await EnsureAppointmentAsync(
            doctor,
            patient,
            completedStatus,
            yesterday,
            new TimeOnly(12, 0),
            new TimeOnly(12, 30),
            "Demo completed appointment with visit record.",
            context);

        await EnsureVisitRecordAndPrescriptionAsync(completedAppointment, context);
    }

    private static async Task<Appointment> EnsureAppointmentAsync(
        Doctor doctor,
        Patient patient,
        AppointmentStatusLookup status,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        string notes,
        ApplicationDbContext context)
    {
        var appointment = await context.Appointments
            .FirstOrDefaultAsync(a =>
                a.DoctorId == doctor.Id &&
                a.AppointmentDate.Date == date.Date &&
                a.StartTime == startTime);

        if (appointment != null)
        {
            appointment.PatientId = patient.Id;
            appointment.StatusId = status.Id;
            appointment.EndTime = endTime;
            appointment.Notes = notes;
            appointment.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return appointment;
        }

        appointment = new Appointment
        {
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            StatusId = status.Id,
            AppointmentDate = date,
            StartTime = startTime,
            EndTime = endTime,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        return appointment;
    }

    private static async Task EnsureVisitRecordAndPrescriptionAsync(
        Appointment appointment,
        ApplicationDbContext context)
    {
        var visitRecord = await context.VisitRecords
            .Include(v => v.Prescriptions)
            .FirstOrDefaultAsync(v => v.AppointmentId == appointment.Id);

        if (visitRecord == null)
        {
            visitRecord = new VisitRecord
            {
                AppointmentId = appointment.Id,
                DoctorNotes = "Patient came for a general consultation. Vital signs were stable.",
                Diagnosis = "Common cold",
                Treatment = "Rest, fluids, and medication as needed.",
                CreatedAt = DateTime.UtcNow
            };

            context.VisitRecords.Add(visitRecord);
            await context.SaveChangesAsync();
        }

        var hasPrescription = await context.Prescriptions
            .AnyAsync(p => p.VisitRecordId == visitRecord.Id);

        if (!hasPrescription)
        {
            context.Prescriptions.Add(new Prescription
            {
                VisitRecordId = visitRecord.Id,
                MedicationName = "Paracetamol",
                Dosage = "500mg",
                Frequency = "Twice daily",
                DurationDays = 3,
                Instructions = "Take after food.",
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureDoctorNotificationsAsync(
        ApplicationUser doctorUser,
        ApplicationDbContext context)
    {
        var generalType = await context.NotificationTypes
            .FirstOrDefaultAsync(t => t.Name == "General");

        var appointmentType = await context.NotificationTypes
            .FirstOrDefaultAsync(t => t.Name == "Appointment");

        var hasWelcomeNotification = await context.Notifications
            .AnyAsync(n => n.UserId == doctorUser.Id && n.Title == "Welcome Doctor");

        if (!hasWelcomeNotification)
        {
            context.Notifications.Add(new Notification
            {
                UserId = doctorUser.Id,
                NotificationTypeId = generalType?.Id,
                Title = "Welcome Doctor",
                Message = "Your doctor dashboard, schedule, appointments, and profile are ready for testing.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        var hasAppointmentNotification = await context.Notifications
            .AnyAsync(n => n.UserId == doctorUser.Id && n.Title == "Appointments Ready");

        if (!hasAppointmentNotification)
        {
            context.Notifications.Add(new Notification
            {
                UserId = doctorUser.Id,
                NotificationTypeId = appointmentType?.Id,
                Title = "Appointments Ready",
                Message = "Demo appointments have been added so you can test the doctor workflow.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }
}