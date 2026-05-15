using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

public static class DbSeeder
{
    // Old method for WebAPI compatibility
    public static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await SeedUsersAndRolesAsync(userManager, roleManager);
    }

    // Main method used by MVC to seed users + application data
    public static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        await SeedUsersAndRolesAsync(userManager, roleManager);
        await SeedApplicationDataAsync(userManager, context);
    }

    // Creates demo users and assigns roles
    private static async Task SeedUsersAndRolesAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var users = new[]
        {
            new { FullName = "Hussain Ali", Email = "hussain@gentlecare.com", Password = "Hussain@123", Role = "ClinicManager" },
            new { FullName = "Sayed Jaffar", Email = "sayedjaffar@gentlecare.com", Password = "Sayed@123", Role = "Receptionist" },

            new { FullName = "Fatema Mohamed", Email = "fatema@gentlecare.com", Password = "Fatema@123", Role = "Doctor" },
            new { FullName = "Hassan Ali", Email = "hassan@gentlecare.com", Password = "Hassan@123", Role = "Doctor" },
            new { FullName = "Ali Mohamed", Email = "ali@gentlecare.com", Password = "Ali@1234", Role = "Doctor" },
            new { FullName = "Jawad Ali", Email = "jawad@gentlecare.com", Password = "Jawad@123", Role = "Doctor" },
            new { FullName = "Masooma Ridha", Email = "masooma@gentlecare.com", Password = "Masooma@123", Role = "Doctor" },

            new { FullName = "Mohamed Baqer", Email = "mohamed@gentlecare.com", Password = "Mohamed@123", Role = "Patient" },
            new { FullName = "Zahraa", Email = "zahraa@gmail.com", Password = "Zahraa@123", Role = "Patient" },
            new { FullName = "Mohsen Ali", Email = "mohsen@gmail.com", Password = "Mohsen@123", Role = "Patient" }
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

                var result = await userManager.CreateAsync(user, u.Password);

                if (result.Succeeded)
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

    // Seeds lookup data, profiles, schedules, appointments, records, and notifications
    private static async Task SeedApplicationDataAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        await EnsureLookupDataAsync(context);

        // More clinic specializations
        var general = await EnsureSpecializationAsync(context, "General Medicine", "General medical consultation and primary care");
        var cardio = await EnsureSpecializationAsync(context, "Cardiology", "Heart and cardiovascular care");
        var derma = await EnsureSpecializationAsync(context, "Dermatology", "Skin care and treatment");
        var pediatrics = await EnsureSpecializationAsync(context, "Pediatrics", "Children healthcare");
        var ortho = await EnsureSpecializationAsync(context, "Orthopedics", "Bones, joints, and muscles");
        var neuro = await EnsureSpecializationAsync(context, "Neurology", "Brain, nerves, and nervous system");
        var ent = await EnsureSpecializationAsync(context, "ENT", "Ear, nose, and throat care");
        var dental = await EnsureSpecializationAsync(context, "Dental Care", "Dental and oral health care");

        // Doctors: each doctor can have many specializations
        var fatema = await EnsureDoctorAsync(userManager, context, "fatema@gentlecare.com", "GC-DOC-1001", "Experienced doctor in general medicine and pediatrics.");
        var hassan = await EnsureDoctorAsync(userManager, context, "hassan@gentlecare.com", "GC-DOC-1002", "Specialist in cardiology and internal medicine.");
        var ali = await EnsureDoctorAsync(userManager, context, "ali@gentlecare.com", "GC-DOC-1003", "Specialist in dermatology and ENT.");
        var jawad = await EnsureDoctorAsync(userManager, context, "jawad@gentlecare.com", "GC-DOC-1004", "Specialist in orthopedics and neurology.");
        var masooma = await EnsureDoctorAsync(userManager, context, "masooma@gentlecare.com", "GC-DOC-1005", "Specialist in pediatrics, dental care, and general medicine.");

        await EnsureDoctorSpecializationsAsync(context, fatema, general.Id, pediatrics.Id, ent.Id);
        await EnsureDoctorSpecializationsAsync(context, hassan, cardio.Id, general.Id, neuro.Id);
        await EnsureDoctorSpecializationsAsync(context, ali, derma.Id, ent.Id, general.Id);
        await EnsureDoctorSpecializationsAsync(context, jawad, ortho.Id, neuro.Id, general.Id);
        await EnsureDoctorSpecializationsAsync(context, masooma, pediatrics.Id, dental.Id, general.Id);

        var doctors = new List<Doctor> { fatema, hassan, ali, jawad, masooma };

        // Patients: each patient will have many appointments
        var mohamed = await EnsurePatientAsync(userManager, context, "mohamed@gentlecare.com", "990000001", "PAT-1001", "O+");
        var zahraa = await EnsurePatientAsync(userManager, context, "zahraa@gmail.com", "990000002", "PAT-1002", "A+");
        var mohsen = await EnsurePatientAsync(userManager, context, "mohsen@gmail.com", "990000003", "PAT-1003", "B+");

        var patients = new List<Patient> { mohamed, zahraa, mohsen };

        // Working schedules for every doctor
        foreach (var doctor in doctors)
        {
            await EnsureDoctorScheduleAsync(doctor, context);
        }

        // More appointments distributed across many doctors and patients
        await EnsureDemoAppointmentsAsync(doctors, patients, context);

        // One doctor on leave for manager dashboard testing
        await EnsureDoctorLeaveAsync(masooma, context);

        // Demo notifications for all users
        await EnsureNotificationsAsync(userManager, context);
    }

    private static async Task EnsureLookupDataAsync(ApplicationDbContext context)
    {
        var statuses = new[]
        {
            new { Name = "Requested", Description = "Appointment has been requested" },
            new { Name = "Confirmed", Description = "Appointment has been confirmed" },
            new { Name = "CheckedIn", Description = "Patient has checked in" },
            new { Name = "InProgress", Description = "Appointment is in progress" },
            new { Name = "Completed", Description = "Appointment has been completed" },
            new { Name = "Cancelled", Description = "Appointment has been cancelled" },
            new { Name = "Missed", Description = "Patient missed the appointment" }
        };

        foreach (var status in statuses)
        {
            if (!await context.AppointmentStatuses.AnyAsync(s => s.Name == status.Name))
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
            if (!await context.NotificationTypes.AnyAsync(t => t.Name == type))
            {
                context.NotificationTypes.Add(new NotificationType
                {
                    Name = type
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Specialization> EnsureSpecializationAsync(
        ApplicationDbContext context,
        string name,
        string description)
    {
        var specialization = await context.Specializations.FirstOrDefaultAsync(s => s.Name == name);

        if (specialization != null)
        {
            specialization.Description = description;
            await context.SaveChangesAsync();
            return specialization;
        }

        specialization = new Specialization
        {
            Name = name,
            Description = description
        };

        context.Specializations.Add(specialization);
        await context.SaveChangesAsync();

        return specialization;
    }

    private static async Task<Doctor> EnsureDoctorAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        string email,
        string licenseNumber,
        string bio)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            throw new Exception($"Seed doctor user not found: {email}");
        }

        var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

        if (doctor == null)
        {
            doctor = new Doctor
            {
                UserId = user.Id,
                LicenseNumber = licenseNumber,
                Bio = bio,
                CreatedAt = DateTime.UtcNow
            };

            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();
        }
        else
        {
            doctor.LicenseNumber = licenseNumber;
            doctor.Bio = bio;
            doctor.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        return doctor;
    }

    // Adds many specializations for one doctor
    private static async Task EnsureDoctorSpecializationsAsync(
        ApplicationDbContext context,
        Doctor doctor,
        params int[] specializationIds)
    {
        foreach (var specializationId in specializationIds)
        {
            var exists = await context.DoctorSpecializations
                .AnyAsync(ds => ds.DoctorId == doctor.Id && ds.SpecializationId == specializationId);

            if (!exists)
            {
                context.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorId = doctor.Id,
                    SpecializationId = specializationId
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Patient> EnsurePatientAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        string email,
        string cpr,
        string reference,
        string bloodType)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            throw new Exception($"Seed patient user not found: {email}");
        }

        // First try to find patient by UserId
        var patient = await context.Patients
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        // If not found by UserId, check if CPR already exists
        if (patient == null)
        {
            patient = await context.Patients
                .FirstOrDefaultAsync(p => p.CPRNumber == cpr);
        }

        // If not found by CPR, check if ReferenceNumber already exists
        if (patient == null)
        {
            patient = await context.Patients
                .FirstOrDefaultAsync(p => p.ReferenceNumber == reference);
        }

        if (patient == null)
        {
            patient = new Patient
            {
                UserId = user.Id,
                CPRNumber = cpr,
                ReferenceNumber = reference,
                DateOfBirth = new DateTime(2000, 1, 1),
                BloodType = bloodType,
                Address = "Bahrain",
                EmergencyContactName = "Emergency Contact",
                EmergencyContactPhone = "39999999",
                CreatedAt = DateTime.UtcNow
            };

            context.Patients.Add(patient);
        }
        else
        {
            patient.UserId = user.Id;
            patient.CPRNumber = cpr;
            patient.ReferenceNumber = reference;
            patient.DateOfBirth = new DateTime(2000, 1, 1);
            patient.BloodType = bloodType;
            patient.Address = "Bahrain";
            patient.EmergencyContactName = "Emergency Contact";
            patient.EmergencyContactPhone = "39999999";
            patient.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();

        return patient;
    }

    // Adds weekly schedules for doctors
    private static async Task EnsureDoctorScheduleAsync(
        Doctor doctor,
        ApplicationDbContext context)
    {
        var hasSchedule = await context.DoctorSchedules.AnyAsync(s => s.DoctorId == doctor.Id);

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
                EndTime = new TimeOnly(15, 0),
                SlotDurationMinutes = 30
            },
            new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(15, 0),
                SlotDurationMinutes = 30
            },
            new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(15, 0),
                SlotDurationMinutes = 30
            },
            new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Wednesday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(15, 0),
                SlotDurationMinutes = 30
            },
            new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Thursday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(15, 0),
                SlotDurationMinutes = 30
            }
        );

        await context.SaveChangesAsync();
    }

    // Creates many appointments so dashboards and history pages look full
    private static async Task EnsureDemoAppointmentsAsync(
        List<Doctor> doctors,
        List<Patient> patients,
        ApplicationDbContext context)
    {
        var requested = await context.AppointmentStatuses.FirstAsync(s => s.Name == "Requested");
        var confirmed = await context.AppointmentStatuses.FirstAsync(s => s.Name == "Confirmed");
        var checkedIn = await context.AppointmentStatuses.FirstAsync(s => s.Name == "CheckedIn");
        var inProgress = await context.AppointmentStatuses.FirstAsync(s => s.Name == "InProgress");
        var completed = await context.AppointmentStatuses.FirstAsync(s => s.Name == "Completed");
        var cancelled = await context.AppointmentStatuses.FirstAsync(s => s.Name == "Cancelled");
        var missed = await context.AppointmentStatuses.FirstAsync(s => s.Name == "Missed");

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);
        var nextWeek = today.AddDays(7);

        // Mohamed Baqer appointments
        await EnsureAppointmentAsync(doctors[0], patients[0], confirmed, today, new TimeOnly(9, 0), new TimeOnly(9, 30), "General consultation confirmed.", context);
        await EnsureAppointmentAsync(doctors[1], patients[0], inProgress, today, new TimeOnly(10, 0), new TimeOnly(10, 30), "Cardiology appointment in progress.", context);
        var mbCompleted1 = await EnsureAppointmentAsync(doctors[2], patients[0], completed, yesterday, new TimeOnly(9, 0), new TimeOnly(9, 30), "Completed dermatology visit.", context);
        var mbCompleted2 = await EnsureAppointmentAsync(doctors[4], patients[0], completed, twoDaysAgo, new TimeOnly(11, 0), new TimeOnly(11, 30), "Completed dental visit.", context);
        await EnsureAppointmentAsync(doctors[3], patients[0], confirmed, tomorrow, new TimeOnly(12, 0), new TimeOnly(12, 30), "Orthopedics follow-up tomorrow.", context);

        // Zahraa appointments
        await EnsureAppointmentAsync(doctors[1], patients[1], checkedIn, today, new TimeOnly(9, 30), new TimeOnly(10, 0), "Patient checked in and waiting.", context);
        await EnsureAppointmentAsync(doctors[3], patients[1], requested, today, new TimeOnly(11, 0), new TimeOnly(11, 30), "Neurology appointment request.", context);
        var zCompleted1 = await EnsureAppointmentAsync(doctors[0], patients[1], completed, yesterday, new TimeOnly(10, 0), new TimeOnly(10, 30), "Completed general visit.", context);
        await EnsureAppointmentAsync(doctors[2], patients[1], cancelled, yesterday, new TimeOnly(13, 0), new TimeOnly(13, 30), "Cancelled dermatology appointment.", context);
        await EnsureAppointmentAsync(doctors[4], patients[1], confirmed, nextWeek, new TimeOnly(9, 0), new TimeOnly(9, 30), "Pediatrics appointment next week.", context);

        // Mohsen Ali appointments
        await EnsureAppointmentAsync(doctors[2], patients[2], confirmed, today, new TimeOnly(12, 0), new TimeOnly(12, 30), "ENT consultation confirmed.", context);
        await EnsureAppointmentAsync(doctors[4], patients[2], confirmed, today, new TimeOnly(13, 0), new TimeOnly(13, 30), "Pediatrics consultation confirmed.", context);
        var moCompleted1 = await EnsureAppointmentAsync(doctors[1], patients[2], completed, yesterday, new TimeOnly(11, 0), new TimeOnly(11, 30), "Completed cardiology visit.", context);
        await EnsureAppointmentAsync(doctors[3], patients[2], missed, yesterday, new TimeOnly(12, 0), new TimeOnly(12, 30), "Patient did not attend.", context);
        await EnsureAppointmentAsync(doctors[0], patients[2], confirmed, tomorrow, new TimeOnly(10, 0), new TimeOnly(10, 30), "General follow-up tomorrow.", context);

        // Add visit records and prescriptions for completed appointments
        await EnsureVisitRecordAndPrescriptionAsync(mbCompleted1, context);
        await EnsureVisitRecordAndPrescriptionAsync(mbCompleted2, context);
        await EnsureVisitRecordAndPrescriptionAsync(zCompleted1, context);
        await EnsureVisitRecordAndPrescriptionAsync(moCompleted1, context);
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

    // Completed appointment creates visit history and prescription
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
                DoctorNotes = "Patient attended the appointment. Vital signs were stable.",
                Diagnosis = "Routine medical condition",
                Treatment = "Medication and follow-up if symptoms continue.",
                CreatedAt = DateTime.UtcNow
            };

            context.VisitRecords.Add(visitRecord);
            await context.SaveChangesAsync();
        }

        var hasPrescription = await context.Prescriptions.AnyAsync(p => p.VisitRecordId == visitRecord.Id);

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

    // Demo leave so Clinic Manager dashboard shows doctor on leave
    private static async Task EnsureDoctorLeaveAsync(
        Doctor doctor,
        ApplicationDbContext context)
    {
        var today = DateTime.Today;

        var exists = await context.DoctorLeaves
            .AnyAsync(l => l.DoctorId == doctor.Id && l.StartDate.Date == today.Date);

        if (exists)
        {
            return;
        }

        context.DoctorLeaves.Add(new DoctorLeave
        {
            DoctorId = doctor.Id,
            StartDate = today,
            EndDate = today,
            Reason = "Demo leave for dashboard testing"
        });

        await context.SaveChangesAsync();
    }

    // Adds unread notifications for demo users
    private static async Task EnsureNotificationsAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        var generalType = await context.NotificationTypes.FirstOrDefaultAsync(t => t.Name == "General");
        var appointmentType = await context.NotificationTypes.FirstOrDefaultAsync(t => t.Name == "Appointment");

        var users = await userManager.Users.ToListAsync();

        foreach (var user in users)
        {
            var hasWelcome = await context.Notifications
                .AnyAsync(n => n.UserId == user.Id && n.Title == "Welcome to GentleCare");

            if (!hasWelcome)
            {
                context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    NotificationTypeId = generalType?.Id,
                    Title = "Welcome to GentleCare",
                    Message = "Your GentleCare account and demo data are ready for testing.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var hasAppointmentUpdate = await context.Notifications
                .AnyAsync(n => n.UserId == user.Id && n.Title == "Appointment Updates Available");

            if (!hasAppointmentUpdate)
            {
                context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    NotificationTypeId = appointmentType?.Id,
                    Title = "Appointment Updates Available",
                    Message = "Appointments have been added for dashboard and workflow testing.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
    }
}