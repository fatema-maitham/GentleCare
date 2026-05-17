using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

public static class DbSeeder
{
    // Used by WebAPI when only users and roles are needed.
    public static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await SeedUsersAndRolesAsync(userManager, roleManager);
    }

    // Used by MVCApp to seed users, roles, and full clinic demo data.
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
        string[] roles =
        {
            "ClinicManager",
            "Receptionist",
            "Doctor",
            "Patient"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var users = new[]
        {
            new { FullName = "Hussain Ali", Email = "hussain@gentlecare.com", Password = "Hussain@123", Role = "ClinicManager" },
            new { FullName = "Sayed Jaffar", Email = "sayedjaffar@gentlecare.com", Password = "Sayed@123", Role = "Receptionist" },

            new { FullName = "Fatema Mohamed", Email = "fatema@gentlecare.com", Password = "Fatema@123", Role = "Doctor" },
            new { FullName = "Hassan Ali", Email = "hassan@gentlecare.com", Password = "Hassan@123", Role = "Doctor" },
            new { FullName = "Ali Mohamed", Email = "ali@gentlecare.com", Password = "Ali@1234", Role = "Doctor" },
            new { FullName = "Jawad Ali", Email = "jawad@gentlecare.com", Password = "Jawad@123", Role = "Doctor" },
            new { FullName = "Masooma Ridha", Email = "masooma@gentlecare.com", Password = "Masooma@123", Role = "Doctor" },
            new { FullName = "Abbas Ali", Email = "abbas@gentlecare.com", Password = "Abbas@123", Role = "Doctor" },

            new { FullName = "Sayed Hassan", Email = "sayedhassan@gmail.com", Password = "Sayed@123", Role = "Patient" },
            new { FullName = "Mohamed Baqer", Email = "mohamed@gmail.com", Password = "Mohamed@123", Role = "Patient" },
            new { FullName = "Zahraa Ahmed", Email = "zahraa@gmail.com", Password = "Zahraa@123", Role = "Patient" },
            new { FullName = "Mohsen Ali", Email = "mohsen@gmail.com", Password = "Mohsen@123", Role = "Patient" },
            new { FullName = "Zainab Abbas", Email = "zainab@gmail.com", Password = "Maryam@123", Role = "Patient" },
            new { FullName = "Sajjad Ali", Email = "sajjad@gmail.com", Password = "Sajjad@123", Role = "Patient" },
            new { FullName = "Mahdi Mohamed", Email = "mahdi@gmail.com", Password = "Mahdi@123", Role = "Patient" },
            new { FullName = "Hadi Ali", Email = "hadi@gmail.com", Password = "Hadi@123", Role = "Patient" }
        };

        foreach (var item in users)
        {
            var user = await userManager.FindByEmailAsync(item.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    FullName = item.FullName,
                    Email = item.Email,
                    UserName = item.Email,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, item.Password);

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create seed user: " + item.Email);
                }
            }
            else
            {
                user.FullName = item.FullName;
                user.EmailConfirmed = true;
                user.IsActive = true;
                await userManager.UpdateAsync(user);
            }

            if (!await userManager.IsInRoleAsync(user, item.Role))
            {
                await userManager.AddToRoleAsync(user, item.Role);
            }
        }
    }

    private static async Task SeedApplicationDataAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        await EnsureLookupDataAsync(context);

        var general = await EnsureSpecializationAsync(context, "General Medicine", "Primary care, routine checkups, and family healthcare.");
        var cardiology = await EnsureSpecializationAsync(context, "Cardiology", "Heart, blood pressure, and cardiovascular care.");
        var dermatology = await EnsureSpecializationAsync(context, "Dermatology", "Skin, allergy, acne, hair, and dermatology care.");
        var pediatrics = await EnsureSpecializationAsync(context, "Pediatrics", "Healthcare for infants, children, and teenagers.");
        var orthopedics = await EnsureSpecializationAsync(context, "Orthopedics", "Bone, joint, muscle, and injury care.");
        var neurology = await EnsureSpecializationAsync(context, "Neurology", "Brain, nerves, headaches, and neurological care.");
        var ent = await EnsureSpecializationAsync(context, "ENT", "Ear, nose, throat, sinus, and hearing care.");
        var dental = await EnsureSpecializationAsync(context, "Dental Care", "Dental, oral hygiene, and tooth-related care.");

        var fatema = await EnsureDoctorAsync(userManager, context, "fatema@gentlecare.com", "GC-DOC-1001", "General medicine doctor focused on family care and routine follow-ups.");
        var hassan = await EnsureDoctorAsync(userManager, context, "hassan@gentlecare.com", "GC-DOC-1002", "Cardiology doctor with experience in blood pressure and heart follow-ups.");
        var ali = await EnsureDoctorAsync(userManager, context, "ali@gentlecare.com", "GC-DOC-1003", "Dermatology and ENT doctor for skin, allergy, sinus, and throat care.");
        var jawad = await EnsureDoctorAsync(userManager, context, "jawad@gentlecare.com", "GC-DOC-1004", "Orthopedics and neurology doctor for pain, injuries, and nerve-related cases.");
        var masooma = await EnsureDoctorAsync(userManager, context, "masooma@gentlecare.com", "GC-DOC-1005", "Pediatrics, dental care, and general medicine doctor.");
        var abbas = await EnsureDoctorAsync(userManager, context, "abbas@gentlecare.com", "GC-DOC-1006", "Neurology, cardiology, and general medicine doctor.");

        // Mixed specialization counts for demo:
        // Fatema has 1, Hassan/Ali/Jawad have 2, Masooma/Abbas have 3.
        await ReplaceDoctorSpecializationsAsync(context, fatema, general.Id);
        await ReplaceDoctorSpecializationsAsync(context, hassan, cardiology.Id, general.Id);
        await ReplaceDoctorSpecializationsAsync(context, ali, dermatology.Id, ent.Id);
        await ReplaceDoctorSpecializationsAsync(context, jawad, orthopedics.Id, neurology.Id);
        await ReplaceDoctorSpecializationsAsync(context, masooma, pediatrics.Id, dental.Id, general.Id);
        await ReplaceDoctorSpecializationsAsync(context, abbas, neurology.Id, cardiology.Id, general.Id);

        var doctors = new List<Doctor>
        {
            fatema,
            hassan,
            ali,
            jawad,
            masooma,
            abbas
        };

        // CPR starts with birth year style: 90 = 1990, 07 = 2007, 00 = 2000.
        var sayedHassan = await EnsurePatientAsync(userManager, context, "sayedhassan@gmail.com", "900101001", "PAT-1001", new DateTime(1990, 1, 1), "O+");
        var mohamed = await EnsurePatientAsync(userManager, context, "mohamed@gmail.com", "980312002", "PAT-1002", new DateTime(1998, 3, 12), "A+");
        var zahraa = await EnsurePatientAsync(userManager, context, "zahraa@gmail.com", "010705003", "PAT-1003", new DateTime(2001, 7, 5), "B+");
        var mohsen = await EnsurePatientAsync(userManager, context, "mohsen@gmail.com", "951122004", "PAT-1004", new DateTime(1995, 11, 22), "AB+");
        var zainab = await EnsurePatientAsync(userManager, context, "zainab@gmail.com", "070218005", "PAT-1005", new DateTime(2007, 2, 18), "O-");
        var sajjad = await EnsurePatientAsync(userManager, context, "sajjad@gmail.com", "890909006", "PAT-1006", new DateTime(1989, 9, 9), "A-");
        var mahdi = await EnsurePatientAsync(userManager, context, "mahdi@gmail.com", "040415007", "PAT-1007", new DateTime(2004, 4, 15), "B-");
        var hadi = await EnsurePatientAsync(userManager, context, "hadi@gmail.com", "001230008", "PAT-1008", new DateTime(2000, 12, 30), "O+");

        var patients = new List<Patient>
        {
            sayedHassan,
            mohamed,
            zahraa,
            mohsen,
            zainab,
            sajjad,
            mahdi,
            hadi
        };

        foreach (var doctor in doctors)
        {
            await EnsureDoctorScheduleAsync(context, doctor);
        }

        await EnsureDoctorLeaveAsync(context, masooma, DateTime.Today.AddDays(4), DateTime.Today.AddDays(4), "Medical conference leave.");
        await EnsureDoctorLeaveAsync(context, jawad, DateTime.Today.AddDays(9), DateTime.Today.AddDays(10), "Planned personal leave.");

        await EnsureDemoAppointmentsAsync(context, doctors, patients);
        await EnsureNotificationsAsync(userManager, context);
    }

    private static async Task EnsureLookupDataAsync(ApplicationDbContext context)
    {
        var statuses = new[]
        {
        new { Name = "Requested", Description = "Appointment has been requested and is waiting for confirmation." },
        new { Name = "Confirmed", Description = "Appointment has been confirmed." },
        new { Name = "CheckedIn", Description = "Patient has checked in and is waiting." },
        new { Name = "InProgress", Description = "Doctor is currently seeing the patient." },
        new { Name = "Completed", Description = "Appointment has been completed." },
        new { Name = "Cancelled", Description = "Appointment has been cancelled." },
        new { Name = "Missed", Description = "Patient missed the appointment." }
    };

        foreach (var status in statuses)
        {
            var exists = await context.AppointmentStatuses
                .AsNoTracking()
                .AnyAsync(s => s.Name == status.Name);

            if (!exists)
            {
                context.AppointmentStatuses.Add(new AppointmentStatusLookup
                {
                    Name = status.Name,
                    Description = status.Description
                });
            }
        }

        var notificationTypes = new[]
        {
        "Appointment",
        "Prescription",
        "General",
        "Announcement",
        "FollowUp"
    };

        foreach (var type in notificationTypes)
        {
            var exists = await context.NotificationTypes
                .AsNoTracking()
                .AnyAsync(t => t.Name == type);

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

    private static async Task<Specialization> EnsureSpecializationAsync(
        ApplicationDbContext context,
        string name,
        string description)
    {
        var specialization = await context.Specializations.FirstOrDefaultAsync(s => s.Name == name);

        if (specialization == null)
        {
            specialization = new Specialization
            {
                Name = name,
                Description = description
            };

            context.Specializations.Add(specialization);
        }
        else
        {
            specialization.Description = description;
        }

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
            throw new Exception("Seed doctor user was not found: " + email);
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
        }
        else
        {
            doctor.LicenseNumber = licenseNumber;
            doctor.Bio = bio;
            doctor.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return doctor;
    }

    private static async Task ReplaceDoctorSpecializationsAsync(
        ApplicationDbContext context,
        Doctor doctor,
        params int[] specializationIds)
    {
        var oldSpecializations = await context.DoctorSpecializations
            .Where(ds => ds.DoctorId == doctor.Id)
            .ToListAsync();

        context.DoctorSpecializations.RemoveRange(oldSpecializations);
        await context.SaveChangesAsync();

        foreach (var specializationId in specializationIds.Distinct())
        {
            context.DoctorSpecializations.Add(new DoctorSpecialization
            {
                DoctorId = doctor.Id,
                SpecializationId = specializationId
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Patient> EnsurePatientAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        string email,
        string cprNumber,
        string referenceNumber,
        DateTime dateOfBirth,
        string bloodType)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            throw new Exception("Seed patient user was not found: " + email);
        }

        var patient = await context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (patient == null)
        {
            patient = await context.Patients.FirstOrDefaultAsync(p =>
                p.CPRNumber == cprNumber ||
                p.ReferenceNumber == referenceNumber);
        }

        if (patient == null)
        {
            patient = new Patient
            {
                UserId = user.Id,
                CPRNumber = cprNumber,
                ReferenceNumber = referenceNumber,
                DateOfBirth = dateOfBirth,
                BloodType = bloodType,
                Address = "Manama, Bahrain",
                EmergencyContactName = "Emergency Contact",
                EmergencyContactPhone = "39999999",
                CreatedAt = DateTime.UtcNow
            };

            context.Patients.Add(patient);
        }
        else
        {
            patient.UserId = user.Id;
            patient.CPRNumber = cprNumber;
            patient.ReferenceNumber = referenceNumber;
            patient.DateOfBirth = dateOfBirth;
            patient.BloodType = bloodType;
            patient.Address = "Manama, Bahrain";
            patient.EmergencyContactName = "Emergency Contact";
            patient.EmergencyContactPhone = "39999999";
            patient.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return patient;
    }

    private static async Task EnsureDoctorScheduleAsync(
        ApplicationDbContext context,
        Doctor doctor)
    {
        var existingSchedules = await context.DoctorSchedules
            .Where(s => s.DoctorId == doctor.Id)
            .ToListAsync();

        if (existingSchedules.Any())
        {
            return;
        }

        var workingDays = new[]
        {
            DayOfWeek.Sunday,
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday
        };

        foreach (var day in workingDays)
        {
            context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = day,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(15, 0),
                SlotDurationMinutes = 30
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDoctorLeaveAsync(
        ApplicationDbContext context,
        Doctor doctor,
        DateTime startDate,
        DateTime endDate,
        string reason)
    {
        var exists = await context.DoctorLeaves.AnyAsync(l =>
            l.DoctorId == doctor.Id &&
            l.StartDate.Date == startDate.Date &&
            l.EndDate.Date == endDate.Date);

        if (exists)
        {
            return;
        }

        context.DoctorLeaves.Add(new DoctorLeave
        {
            DoctorId = doctor.Id,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Reason = reason
        });

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDemoAppointmentsAsync(
        ApplicationDbContext context,
        List<Doctor> doctors,
        List<Patient> patients)
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
        var afterTwoDays = today.AddDays(2);
        var afterThreeDays = today.AddDays(3);
        var nextWeek = today.AddDays(7);
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);
        var threeDaysAgo = today.AddDays(-3);
        var fourDaysAgo = today.AddDays(-4);
        var fiveDaysAgo = today.AddDays(-5);

        // Today appointments for live queue and dashboards. Count: 10
        await EnsureAppointmentAsync(context, doctors[0], patients[0], confirmed, today, new TimeOnly(9, 0), new TimeOnly(9, 30), "General consultation confirmed.");
        await EnsureAppointmentAsync(context, doctors[0], patients[1], checkedIn, today, new TimeOnly(9, 30), new TimeOnly(10, 0), "Patient checked in and waiting.");
        await EnsureAppointmentAsync(context, doctors[1], patients[2], inProgress, today, new TimeOnly(10, 0), new TimeOnly(10, 30), "Cardiology appointment in progress.");
        await EnsureAppointmentAsync(context, doctors[1], patients[3], confirmed, today, new TimeOnly(10, 30), new TimeOnly(11, 0), "Blood pressure follow-up confirmed.");
        await EnsureAppointmentAsync(context, doctors[2], patients[4], requested, today, new TimeOnly(11, 0), new TimeOnly(11, 30), "Dermatology appointment request.");
        await EnsureAppointmentAsync(context, doctors[2], patients[5], confirmed, today, new TimeOnly(11, 30), new TimeOnly(12, 0), "ENT consultation confirmed.");
        await EnsureAppointmentAsync(context, doctors[3], patients[6], checkedIn, today, new TimeOnly(12, 0), new TimeOnly(12, 30), "Orthopedics patient checked in.");
        await EnsureAppointmentAsync(context, doctors[3], patients[7], confirmed, today, new TimeOnly(12, 30), new TimeOnly(13, 0), "Neurology consultation confirmed.");
        await EnsureAppointmentAsync(context, doctors[4], patients[0], requested, today, new TimeOnly(13, 0), new TimeOnly(13, 30), "Pediatrics request for child follow-up.");
        await EnsureAppointmentAsync(context, doctors[5], patients[1], confirmed, today, new TimeOnly(13, 30), new TimeOnly(14, 0), "General review confirmed.");

        // Upcoming appointments. Count: 10
        await EnsureAppointmentAsync(context, doctors[0], patients[2], confirmed, tomorrow, new TimeOnly(9, 0), new TimeOnly(9, 30), "General follow-up tomorrow.");
        await EnsureAppointmentAsync(context, doctors[1], patients[3], requested, tomorrow, new TimeOnly(9, 30), new TimeOnly(10, 0), "Cardiology appointment request.");
        await EnsureAppointmentAsync(context, doctors[2], patients[4], confirmed, tomorrow, new TimeOnly(10, 0), new TimeOnly(10, 30), "Skin allergy follow-up.");
        await EnsureAppointmentAsync(context, doctors[3], patients[5], confirmed, tomorrow, new TimeOnly(10, 30), new TimeOnly(11, 0), "Knee pain follow-up.");
        await EnsureAppointmentAsync(context, doctors[4], patients[6], requested, afterTwoDays, new TimeOnly(9, 0), new TimeOnly(9, 30), "Dental cleaning request.");
        await EnsureAppointmentAsync(context, doctors[5], patients[7], confirmed, afterTwoDays, new TimeOnly(9, 30), new TimeOnly(10, 0), "Neurology review.");
        await EnsureAppointmentAsync(context, doctors[0], patients[4], confirmed, afterThreeDays, new TimeOnly(11, 0), new TimeOnly(11, 30), "Routine checkup.");
        await EnsureAppointmentAsync(context, doctors[1], patients[5], confirmed, afterThreeDays, new TimeOnly(11, 30), new TimeOnly(12, 0), "Heart health review.");
        await EnsureAppointmentAsync(context, doctors[2], patients[6], requested, nextWeek, new TimeOnly(12, 0), new TimeOnly(12, 30), "ENT request next week.");
        await EnsureAppointmentAsync(context, doctors[5], patients[0], confirmed, nextWeek, new TimeOnly(12, 30), new TimeOnly(13, 0), "General medicine review next week.");

        // Completed appointments with visit records and prescriptions. Count: 16
        var c1 = await EnsureAppointmentAsync(context, doctors[0], patients[0], completed, yesterday, new TimeOnly(9, 0), new TimeOnly(9, 30), "Completed general visit.");
        var c2 = await EnsureAppointmentAsync(context, doctors[1], patients[0], completed, twoDaysAgo, new TimeOnly(10, 0), new TimeOnly(10, 30), "Completed cardiology visit.");
        var c3 = await EnsureAppointmentAsync(context, doctors[2], patients[1], completed, yesterday, new TimeOnly(10, 30), new TimeOnly(11, 0), "Completed dermatology visit.");
        var c4 = await EnsureAppointmentAsync(context, doctors[3], patients[1], completed, threeDaysAgo, new TimeOnly(11, 0), new TimeOnly(11, 30), "Completed orthopedics visit.");
        var c5 = await EnsureAppointmentAsync(context, doctors[4], patients[2], completed, twoDaysAgo, new TimeOnly(11, 30), new TimeOnly(12, 0), "Completed dental visit.");
        var c6 = await EnsureAppointmentAsync(context, doctors[5], patients[2], completed, fourDaysAgo, new TimeOnly(12, 0), new TimeOnly(12, 30), "Completed neurology visit.");
        var c7 = await EnsureAppointmentAsync(context, doctors[0], patients[3], completed, yesterday, new TimeOnly(12, 30), new TimeOnly(13, 0), "Completed routine checkup.");
        var c8 = await EnsureAppointmentAsync(context, doctors[1], patients[3], completed, fiveDaysAgo, new TimeOnly(13, 0), new TimeOnly(13, 30), "Completed blood pressure follow-up.");
        var c9 = await EnsureAppointmentAsync(context, doctors[2], patients[4], completed, twoDaysAgo, new TimeOnly(13, 30), new TimeOnly(14, 0), "Completed ENT visit.");
        var c10 = await EnsureAppointmentAsync(context, doctors[3], patients[4], completed, fourDaysAgo, new TimeOnly(14, 0), new TimeOnly(14, 30), "Completed back pain visit.");
        var c11 = await EnsureAppointmentAsync(context, doctors[4], patients[5], completed, threeDaysAgo, new TimeOnly(9, 0), new TimeOnly(9, 30), "Completed pediatrics visit.");
        var c12 = await EnsureAppointmentAsync(context, doctors[5], patients[5], completed, fiveDaysAgo, new TimeOnly(9, 30), new TimeOnly(10, 0), "Completed headache consultation.");
        var c13 = await EnsureAppointmentAsync(context, doctors[0], patients[6], completed, twoDaysAgo, new TimeOnly(10, 0), new TimeOnly(10, 30), "Completed general checkup.");
        var c14 = await EnsureAppointmentAsync(context, doctors[1], patients[6], completed, fourDaysAgo, new TimeOnly(10, 30), new TimeOnly(11, 0), "Completed ECG follow-up.");
        var c15 = await EnsureAppointmentAsync(context, doctors[2], patients[7], completed, yesterday, new TimeOnly(11, 0), new TimeOnly(11, 30), "Completed skin irritation visit.");
        var c16 = await EnsureAppointmentAsync(context, doctors[5], patients[7], completed, threeDaysAgo, new TimeOnly(11, 30), new TimeOnly(12, 0), "Completed general medical review.");

        // Cancelled appointments for reports. Count: 5
        await EnsureAppointmentAsync(context, doctors[0], patients[1], cancelled, yesterday, new TimeOnly(14, 0), new TimeOnly(14, 30), "Cancelled by patient.", "Patient requested cancellation.");
        await EnsureAppointmentAsync(context, doctors[1], patients[2], cancelled, twoDaysAgo, new TimeOnly(14, 30), new TimeOnly(15, 0), "Cancelled due to schedule conflict.", "Doctor schedule changed.");
        await EnsureAppointmentAsync(context, doctors[2], patients[3], cancelled, threeDaysAgo, new TimeOnly(9, 0), new TimeOnly(9, 30), "Cancelled appointment.", "Patient was unavailable.");
        await EnsureAppointmentAsync(context, doctors[3], patients[4], cancelled, fourDaysAgo, new TimeOnly(9, 30), new TimeOnly(10, 0), "Cancelled appointment.", "Clinic rescheduling.");
        await EnsureAppointmentAsync(context, doctors[5], patients[6], cancelled, fiveDaysAgo, new TimeOnly(10, 0), new TimeOnly(10, 30), "Cancelled appointment.", "Patient requested another date.");

        // Missed appointments for reports. Count: 4
        await EnsureAppointmentAsync(context, doctors[3], patients[2], missed, yesterday, new TimeOnly(14, 30), new TimeOnly(15, 0), "Patient did not attend.");
        await EnsureAppointmentAsync(context, doctors[4], patients[3], missed, twoDaysAgo, new TimeOnly(14, 0), new TimeOnly(14, 30), "Patient missed dental appointment.");
        await EnsureAppointmentAsync(context, doctors[1], patients[5], missed, threeDaysAgo, new TimeOnly(14, 30), new TimeOnly(15, 0), "Patient did not attend cardiology appointment.");
        await EnsureAppointmentAsync(context, doctors[2], patients[7], missed, fourDaysAgo, new TimeOnly(14, 30), new TimeOnly(15, 0), "Patient missed ENT appointment.");

        await EnsureVisitRecordAndPrescriptionAsync(context, c1, "Seasonal flu", "Rest, fluids, and fever control.", "Paracetamol", "500mg", "Twice daily", 3);
        await EnsureVisitRecordAndPrescriptionAsync(context, c2, "High blood pressure follow-up", "Monitor blood pressure and reduce salt intake.", "Amlodipine", "5mg", "Once daily", 30);
        await EnsureVisitRecordAndPrescriptionAsync(context, c3, "Skin allergy", "Use cream and avoid allergen exposure.", "Hydrocortisone Cream", "Apply thin layer", "Twice daily", 7);
        await EnsureVisitRecordAndPrescriptionAsync(context, c4, "Knee pain", "Physiotherapy and pain management.", "Ibuprofen", "400mg", "Twice daily after food", 5);
        await EnsureVisitRecordAndPrescriptionAsync(context, c5, "Dental inflammation", "Dental cleaning and antibiotic course.", "Amoxicillin", "500mg", "Three times daily", 5);
        await EnsureVisitRecordAndPrescriptionAsync(context, c6, "Migraine headache", "Avoid triggers and follow medication plan.", "Sumatriptan", "50mg", "When needed", 3);
        await EnsureVisitRecordAndPrescriptionAsync(context, c7, "Routine checkup", "No major issue. Continue healthy lifestyle.", "Vitamin D", "1000 IU", "Once daily", 30);
        await EnsureVisitRecordAndPrescriptionAsync(context, c8, "Blood pressure review", "Continue lifestyle changes and medication.", "Losartan", "50mg", "Once daily", 30);
        await EnsureVisitRecordAndPrescriptionAsync(context, c9, "Sinus infection", "Steam inhalation and medication.", "Cetirizine", "10mg", "Once daily", 7);
        await EnsureVisitRecordAndPrescriptionAsync(context, c10, "Lower back pain", "Rest, posture care, and physiotherapy.", "Diclofenac Gel", "Apply locally", "Twice daily", 7);
        await EnsureVisitRecordAndPrescriptionAsync(context, c11, "Child fever", "Hydration and fever management.", "Paracetamol Syrup", "10ml", "Every 8 hours", 3);
        await EnsureVisitRecordAndPrescriptionAsync(context, c12, "Tension headache", "Rest and reduce screen exposure.", "Ibuprofen", "200mg", "Twice daily after food", 3);
        await EnsureVisitRecordAndPrescriptionAsync(context, c13, "General fatigue", "Blood test recommended and vitamins prescribed.", "Multivitamin", "One tablet", "Once daily", 30);
        await EnsureVisitRecordAndPrescriptionAsync(context, c14, "Heart rhythm follow-up", "ECG reviewed and follow-up scheduled.", "Aspirin", "81mg", "Once daily", 30);
        await EnsureVisitRecordAndPrescriptionAsync(context, c15, "Skin irritation", "Use prescribed cream and avoid scented products.", "Antihistamine Cream", "Apply thin layer", "Twice daily", 5);
        await EnsureVisitRecordAndPrescriptionAsync(context, c16, "General medical review", "Patient stable. Follow up if symptoms return.", "Vitamin C", "500mg", "Once daily", 14);
    }

    private static async Task<Appointment> EnsureAppointmentAsync(
        ApplicationDbContext context,
        Doctor doctor,
        Patient patient,
        AppointmentStatusLookup status,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        string notes,
        string? cancellationReason = null)
    {
        var appointment = await context.Appointments.FirstOrDefaultAsync(a =>
            a.DoctorId == doctor.Id &&
            a.AppointmentDate.Date == date.Date &&
            a.StartTime == startTime);

        if (appointment == null)
        {
            appointment = new Appointment
            {
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                StatusId = status.Id,
                AppointmentDate = date.Date,
                StartTime = startTime,
                EndTime = endTime,
                Notes = notes,
                CancellationReason = cancellationReason,
                CreatedAt = DateTime.UtcNow
            };

            context.Appointments.Add(appointment);
        }
        else
        {
            appointment.PatientId = patient.Id;
            appointment.StatusId = status.Id;
            appointment.EndTime = endTime;
            appointment.Notes = notes;
            appointment.CancellationReason = cancellationReason;
            appointment.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return appointment;
    }

    private static async Task EnsureVisitRecordAndPrescriptionAsync(
        ApplicationDbContext context,
        Appointment appointment,
        string diagnosis,
        string treatment,
        string medicationName,
        string dosage,
        string frequency,
        int durationDays)
    {
        var visitRecord = await context.VisitRecords
            .Include(v => v.Prescriptions)
            .FirstOrDefaultAsync(v => v.AppointmentId == appointment.Id);

        if (visitRecord == null)
        {
            visitRecord = new VisitRecord
            {
                AppointmentId = appointment.Id,
                DoctorNotes = "Patient attended the appointment. Examination completed and record saved.",
                Diagnosis = diagnosis,
                Treatment = treatment,
                CreatedAt = DateTime.UtcNow
            };

            context.VisitRecords.Add(visitRecord);
            await context.SaveChangesAsync();
        }
        else
        {
            visitRecord.DoctorNotes = "Patient attended the appointment. Examination completed and record saved.";
            visitRecord.Diagnosis = diagnosis;
            visitRecord.Treatment = treatment;
        }

        var hasPrescription = await context.Prescriptions.AnyAsync(p =>
            p.VisitRecordId == visitRecord.Id &&
            p.MedicationName == medicationName);

        if (!hasPrescription)
        {
            context.Prescriptions.Add(new Prescription
            {
                VisitRecordId = visitRecord.Id,
                MedicationName = medicationName,
                Dosage = dosage,
                Frequency = frequency,
                DurationDays = durationDays,
                Instructions = "Follow the doctor's instructions and complete the prescribed duration.",
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureNotificationsAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        await DeleteOldSeedNotificationsAsync(context);

        var appointmentType = await context.NotificationTypes.FirstOrDefaultAsync(t => t.Name == "Appointment");
        var prescriptionType = await context.NotificationTypes.FirstOrDefaultAsync(t => t.Name == "Prescription");
        var generalType = await context.NotificationTypes.FirstOrDefaultAsync(t => t.Name == "General");

        var appointments = await context.Appointments
            .Include(a => a.Status)
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.VisitRecord!)
                .ThenInclude(v => v.Prescriptions)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        foreach (var appointment in appointments)
        {
            var patientUserId = appointment.Patient.UserId;
            var doctorUserId = appointment.Doctor.UserId;
            var patientName = appointment.Patient.User.FullName;
            var doctorName = appointment.Doctor.User.FullName;
            var dateText = FormatAppointmentDate(appointment.AppointmentDate);
            var timeText = appointment.StartTime.ToString("hh:mm tt");
            var statusName = appointment.Status.Name;

            if (statusName == "Requested")
            {
                await EnsureNotificationAsync(
                    context,
                    patientUserId,
                    appointmentType?.Id,
                    "Appointment Request Received",
                    $"Your appointment request with Dr. {doctorName} for {dateText} at {timeText} has been received and is waiting for confirmation.",
                    appointment.Id,
                    "Appointment");

                await EnsureNotificationAsync(
                    context,
                    doctorUserId,
                    appointmentType?.Id,
                    "New Appointment Request",
                    $"{patientName} requested an appointment for {dateText} at {timeText}.",
                    appointment.Id,
                    "Appointment");
            }
            else if (statusName == "Confirmed")
            {
                var patientTitle = appointment.AppointmentDate.Date == DateTime.Today
                    ? "Your Appointment Is Today"
                    : appointment.AppointmentDate.Date == DateTime.Today.AddDays(1)
                        ? "Your Appointment Is Tomorrow"
                        : "Upcoming Appointment Confirmed";

                await EnsureNotificationAsync(
                    context,
                    patientUserId,
                    appointmentType?.Id,
                    patientTitle,
                    $"Your appointment with Dr. {doctorName} is confirmed for {dateText} at {timeText}.",
                    appointment.Id,
                    "Appointment");

                await EnsureNotificationAsync(
                    context,
                    doctorUserId,
                    appointmentType?.Id,
                    "Confirmed Appointment Scheduled",
                    $"You have a confirmed appointment with {patientName} on {dateText} at {timeText}.",
                    appointment.Id,
                    "Appointment");
            }
            else if (statusName == "CheckedIn")
            {
                await EnsureNotificationAsync(
                    context,
                    patientUserId,
                    appointmentType?.Id,
                    "You Are Checked In",
                    $"You have checked in for your appointment with Dr. {doctorName}. Please wait until the doctor starts the visit.",
                    appointment.Id,
                    "Appointment");

                await EnsureNotificationAsync(
                    context,
                    doctorUserId,
                    appointmentType?.Id,
                    "Patient Checked In",
                    $"{patientName} has checked in and is waiting for the appointment.",
                    appointment.Id,
                    "Appointment");
            }
            else if (statusName == "InProgress")
            {
                await EnsureNotificationAsync(
                    context,
                    patientUserId,
                    appointmentType?.Id,
                    "Appointment In Progress",
                    $"Your appointment with Dr. {doctorName} is currently in progress.",
                    appointment.Id,
                    "Appointment");

                await EnsureNotificationAsync(
                    context,
                    doctorUserId,
                    appointmentType?.Id,
                    "Appointment In Progress",
                    $"Your appointment with {patientName} is currently in progress.",
                    appointment.Id,
                    "Appointment");
            }
            else if (statusName == "Completed")
            {
                await EnsureNotificationAsync(
                    context,
                    patientUserId,
                    appointmentType?.Id,
                    "Appointment Completed",
                    $"Your appointment with Dr. {doctorName} was completed. Your visit summary is available in your medical history.",
                    appointment.Id,
                    "Appointment");

                await EnsureNotificationAsync(
                    context,
                    doctorUserId,
                    appointmentType?.Id,
                    "Visit Completed",
                    $"The appointment with {patientName} was completed and the visit record can be reviewed.",
                    appointment.Id,
                    "Appointment");

                if (appointment.VisitRecord != null && appointment.VisitRecord.Prescriptions.Any())
                {
                    foreach (var prescription in appointment.VisitRecord.Prescriptions)
                    {
                        await EnsureNotificationAsync(
                            context,
                            patientUserId,
                            prescriptionType?.Id,
                            "New Prescription Added",
                            $"Dr. {doctorName} added a prescription for {prescription.MedicationName}. Dosage: {prescription.Dosage}, Frequency: {prescription.Frequency}, Duration: {prescription.DurationDays} days.",
                            prescription.Id,
                            "Prescription");

                        await EnsureNotificationAsync(
                            context,
                            doctorUserId,
                            prescriptionType?.Id,
                            "Prescription Saved",
                            $"Prescription for {patientName} was saved: {prescription.MedicationName}, {prescription.Dosage}, {prescription.Frequency}.",
                            prescription.Id,
                            "Prescription");
                    }
                }
            }
            else if (statusName == "Cancelled")
            {
                var reason = string.IsNullOrWhiteSpace(appointment.CancellationReason)
                    ? "No reason was provided."
                    : appointment.CancellationReason;

                await EnsureNotificationAsync(
                    context,
                    patientUserId,
                    appointmentType?.Id,
                    "Appointment Cancelled",
                    $"Your appointment with Dr. {doctorName} on {dateText} at {timeText} was cancelled. Reason: {reason}",
                    appointment.Id,
                    "Appointment");

                await EnsureNotificationAsync(
                    context,
                    doctorUserId,
                    appointmentType?.Id,
                    "Appointment Cancelled",
                    $"The appointment with {patientName} on {dateText} at {timeText} was cancelled. Reason: {reason}",
                    appointment.Id,
                    "Appointment");
            }
            else if (statusName == "Missed")
            {
                await EnsureNotificationAsync(
                    context,
                    patientUserId,
                    appointmentType?.Id,
                    "Appointment Marked As Missed",
                    $"You missed your appointment with Dr. {doctorName} on {dateText} at {timeText}. Please contact the clinic if you need to book again.",
                    appointment.Id,
                    "Appointment");

                await EnsureNotificationAsync(
                    context,
                    doctorUserId,
                    appointmentType?.Id,
                    "Patient Missed Appointment",
                    $"{patientName} missed the appointment scheduled on {dateText} at {timeText}.",
                    appointment.Id,
                    "Appointment");
            }
        }

        var today = DateTime.Today;
        var todayCount = appointments.Count(a => a.AppointmentDate.Date == today);
        var tomorrowCount = appointments.Count(a => a.AppointmentDate.Date == today.AddDays(1));
        var requestedCount = appointments.Count(a => a.Status.Name == "Requested");
        var cancelledCount = appointments.Count(a => a.Status.Name == "Cancelled");
        var missedCount = appointments.Count(a => a.Status.Name == "Missed");
        var completedCount = appointments.Count(a => a.Status.Name == "Completed");

        var clinicManagers = await userManager.GetUsersInRoleAsync("ClinicManager");
        var receptionists = await userManager.GetUsersInRoleAsync("Receptionist");

        foreach (var manager in clinicManagers)
        {
            await EnsureNotificationAsync(
                context,
                manager.Id,
                generalType?.Id,
                "Today’s Live Queue Is Ready",
                $"There are {todayCount} appointments scheduled for today. Check the live queue and appointment dashboard for current progress.",
                null,
                "ManagerSummary");

            await EnsureNotificationAsync(
                context,
                manager.Id,
                generalType?.Id,
                "Appointment Reports Updated",
                $"Clinic reports now include {completedCount} completed appointments, {cancelledCount} cancelled appointments, and {missedCount} missed appointments.",
                null,
                "ManagerSummary");

            await EnsureNotificationAsync(
                context,
                manager.Id,
                generalType?.Id,
                "Pending Requests Need Review",
                $"There are {requestedCount} appointment requests waiting for confirmation.",
                null,
                "ManagerSummary");
        }

        foreach (var receptionist in receptionists)
        {
            await EnsureNotificationAsync(
                context,
                receptionist.Id,
                appointmentType?.Id,
                "Today’s Appointment Queue",
                $"There are {todayCount} appointments scheduled for today. Use the live queue to follow patient check-in and progress.",
                null,
                "ReceptionistSummary");

            await EnsureNotificationAsync(
                context,
                receptionist.Id,
                appointmentType?.Id,
                "Tomorrow’s Appointments",
                $"There are {tomorrowCount} appointments scheduled for tomorrow.",
                null,
                "ReceptionistSummary");

            await EnsureNotificationAsync(
                context,
                receptionist.Id,
                appointmentType?.Id,
                "Appointment Requests Waiting",
                $"There are {requestedCount} appointment requests that may need confirmation.",
                null,
                "ReceptionistSummary");
        }

        var doctorLeaves = await context.DoctorLeaves
            .Include(l => l.Doctor)
                .ThenInclude(d => d.User)
            .ToListAsync();

        foreach (var leave in doctorLeaves)
        {
            foreach (var manager in clinicManagers)
            {
                await EnsureNotificationAsync(
                    context,
                    manager.Id,
                    generalType?.Id,
                    "Doctor Leave Scheduled",
                    $"Dr. {leave.Doctor.User.FullName} has leave from {leave.StartDate:dd MMM yyyy} to {leave.EndDate:dd MMM yyyy}. Reason: {leave.Reason}",
                    leave.Id,
                    "DoctorLeave");
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task DeleteOldSeedNotificationsAsync(ApplicationDbContext context)
    {
        var oldTitles = new[]
        {
            "Welcome to GentleCare",
            "Appointment Updates Available",
            "Prescription Record Available"
        };

        var oldNotifications = await context.Notifications
            .Where(n => oldTitles.Contains(n.Title))
            .ToListAsync();

        if (oldNotifications.Any())
        {
            context.Notifications.RemoveRange(oldNotifications);
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureNotificationAsync(
        ApplicationDbContext context,
        string userId,
        int? notificationTypeId,
        string title,
        string message,
        int? relatedEntityId,
        string? relatedEntityType)
    {
        var exists = await context.Notifications.AnyAsync(n =>
            n.UserId == userId &&
            n.Title == title &&
            n.RelatedEntityId == relatedEntityId &&
            n.RelatedEntityType == relatedEntityType);

        if (exists)
        {
            return;
        }

        context.Notifications.Add(new Notification
        {
            UserId = userId,
            NotificationTypeId = notificationTypeId,
            Title = title,
            Message = message,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static string FormatAppointmentDate(DateTime date)
    {
        if (date.Date == DateTime.Today)
        {
            return "today";
        }

        if (date.Date == DateTime.Today.AddDays(1))
        {
            return "tomorrow";
        }

        if (date.Date == DateTime.Today.AddDays(-1))
        {
            return "yesterday";
        }

        return date.ToString("dd MMM yyyy");
    }
}
