using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.ClinicManager;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Services
{
    // Service for all Clinic Manager business logic.
    // Controllers should call this service instead of writing database logic directly.
    public class ClinicManagerService : IClinicManagerService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAppointmentWorkflowService _workflowService;
        private readonly INotificationService _notificationService;

        private static class StatusNames
        {
            public const string Requested = "Requested";
            public const string Confirmed = "Confirmed";
            public const string CheckedIn = "CheckedIn";
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string Missed = "Missed";
        }

        public ClinicManagerService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAppointmentWorkflowService workflowService,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _workflowService = workflowService;
            _notificationService = notificationService;
        }

        // =========================
        // Dashboard
        // =========================

        public async Task<ClinicManagerDashboardViewModel> GetDashboardAsync()
        {
            var today = DateTime.Today;

            var todayAppointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Where(a => a.AppointmentDate.Date == today)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            var totalToday = todayAppointments.Count;

            var completedToday = todayAppointments.Count(a => a.Status.Name == StatusNames.Completed);
            var cancelledToday = todayAppointments.Count(a => a.Status.Name == StatusNames.Cancelled);
            var missedToday = todayAppointments.Count(a => a.Status.Name == StatusNames.Missed);

            var doctors = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Leaves)
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Status)
                .ToListAsync();

            var appointmentsForImpact = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Status)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Schedules)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Leaves)
                .Where(a =>
                    a.AppointmentDate.Date == today &&
                    a.Status.Name != StatusNames.Completed &&
                    a.Status.Name != StatusNames.Cancelled &&
                    a.Status.Name != StatusNames.Missed)
                .ToListAsync();

            return new ClinicManagerDashboardViewModel
            {
                TotalDoctors = doctors.Count,
                ActiveDoctors = doctors.Count(d => d.User.IsActive),
                TotalPatients = await _context.Patients.CountAsync(),

                AppointmentsToday = totalToday,
                RequestedAppointments = todayAppointments.Count(a => a.Status.Name == StatusNames.Requested),
                ConfirmedAppointments = todayAppointments.Count(a => a.Status.Name == StatusNames.Confirmed),
                CheckedInAppointments = todayAppointments.Count(a => a.Status.Name == StatusNames.CheckedIn),
                InProgressAppointments = todayAppointments.Count(a => a.Status.Name == StatusNames.InProgress),
                CompletedAppointmentsToday = completedToday,
                CancelledAppointmentsToday = cancelledToday,
                MissedAppointmentsToday = missedToday,

                DoctorsOnLeaveToday = doctors.Count(d => IsDoctorOnLeaveToday(d.Leaves)),
                ImpactedAppointmentsToday = appointmentsForImpact.Count(a =>
                    IsAppointmentImpacted(a, a.Doctor.Schedules.ToList(), a.Doctor.Leaves.ToList())),

                CompletionRateToday = CalculateRate(completedToday, totalToday),
                CancellationRateToday = CalculateRate(cancelledToday, totalToday),
                MissedRateToday = CalculateRate(missedToday, totalToday),

                TodayAppointments = todayAppointments.Select(a => new ClinicManagerDashboardAppointmentViewModel
                {
                    AppointmentId = a.Id,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StatusName = _workflowService.FormatStatusName(a.Status.Name),
                    Notes = a.Notes ?? string.Empty
                }).ToList(),

                DoctorWorkloads = doctors.Select(d =>
                {
                    var doctorTodayAppointments = d.Appointments
                        .Where(a => a.AppointmentDate.Date == today)
                        .ToList();

                    return new ClinicManagerDashboardDoctorWorkloadViewModel
                    {
                        DoctorId = d.Id,
                        DoctorName = d.User.FullName,
                        TotalAppointmentsToday = doctorTodayAppointments.Count,
                        CompletedAppointmentsToday = doctorTodayAppointments.Count(a => a.Status.Name == StatusNames.Completed),
                        RemainingAppointmentsToday = doctorTodayAppointments.Count(a =>
                            a.Status.Name != StatusNames.Completed &&
                            a.Status.Name != StatusNames.Cancelled &&
                            a.Status.Name != StatusNames.Missed)
                    };
                })
                .OrderByDescending(d => d.TotalAppointmentsToday)
                .Take(5)
                .ToList(),

                DoctorsOnLeave = doctors
                    .SelectMany(d => d.Leaves
                        .Where(l => l.StartDate.Date <= today && l.EndDate.Date >= today)
                        .Select(l => new ClinicManagerDashboardLeaveViewModel
                        {
                            DoctorLeaveId = l.Id,
                            DoctorId = d.Id,
                            DoctorName = d.User.FullName,
                            LeaveReason = l.Reason ?? string.Empty,
                            LeaveStartDate = l.StartDate,
                            LeaveEndDate = l.EndDate
                        }))
                    .ToList()
            };
        }

        // =========================
        // Doctor Management
        // =========================

        public async Task<ClinicManagerDoctorListViewModel> GetDoctorsAsync(string? searchTerm, bool? isActive)
        {
            var today = DateTime.Today;

            var query = _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Leaves)
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Status)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();

                query = query.Where(d =>
                    d.User.FullName.ToLower().Contains(term) ||
                    (d.User.Email ?? string.Empty).ToLower().Contains(term) ||
                    d.LicenseNumber.ToLower().Contains(term));
            }

            if (isActive.HasValue)
            {
                query = query.Where(d => d.User.IsActive == isActive.Value);
            }

            var doctors = await query
                .OrderBy(d => d.User.FullName)
                .ToListAsync();

            return new ClinicManagerDoctorListViewModel
            {
                SearchTerm = searchTerm,
                IsActive = isActive,

                Doctors = doctors.Select(d => new ClinicManagerDoctorListItemViewModel
                {
                    DoctorId = d.Id,
                    UserId = d.UserId,
                    FullName = d.User.FullName,
                    Email = d.User.Email ?? string.Empty,
                    PhoneNumber = d.User.PhoneNumber ?? string.Empty,
                    LicenseNumber = d.LicenseNumber,
                    Bio = d.Bio ?? string.Empty,
                    IsActive = d.User.IsActive,

                    Specializations = d.DoctorSpecializations
                        .Select(ds => ds.Specialization.Name)
                        .OrderBy(name => name)
                        .ToList(),

                    TodayAppointmentsCount = d.Appointments.Count(a => a.AppointmentDate.Date == today),

                    UpcomingAppointmentsCount = d.Appointments.Count(a =>
                        a.AppointmentDate.Date >= today &&
                        a.Status.Name != StatusNames.Completed &&
                        a.Status.Name != StatusNames.Cancelled &&
                        a.Status.Name != StatusNames.Missed),

                    CompletedAppointmentsCount = d.Appointments.Count(a =>
                        a.Status.Name == StatusNames.Completed),

                    IsOnLeaveToday = IsDoctorOnLeaveToday(d.Leaves)
                }).ToList()
            };
        }

        public async Task<ClinicManagerDoctorDetailsViewModel?> GetDoctorDetailsAsync(int doctorId)
        {
            var today = DateTime.Today;

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Status)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return null;
            }

            return new ClinicManagerDoctorDetailsViewModel
            {
                DoctorId = doctor.Id,
                UserId = doctor.UserId,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? string.Empty,
                PhoneNumber = doctor.User.PhoneNumber ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio ?? string.Empty,
                IsActive = doctor.User.IsActive,
                CreatedAt = doctor.CreatedAt,
                UpdatedAt = doctor.UpdatedAt,

                Specializations = doctor.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name)
                    .OrderBy(name => name)
                    .ToList(),

                Schedules = doctor.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new DoctorScheduleItemViewModel
                    {
                        DoctorScheduleId = s.Id,
                        DoctorId = s.DoctorId,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        SlotDurationMinutes = s.SlotDurationMinutes
                    })
                    .ToList(),

                Leaves = doctor.Leaves
                    .OrderByDescending(l => l.StartDate)
                    .Select(l => new DoctorLeaveItemViewModel
                    {
                        DoctorLeaveId = l.Id,
                        DoctorId = l.DoctorId,
                        LeaveStartDate = l.StartDate,
                        LeaveEndDate = l.EndDate,
                        LeaveReason = l.Reason ?? string.Empty
                    })
                    .ToList(),

                TotalAppointments = doctor.Appointments.Count,
                TodayAppointments = doctor.Appointments.Count(a => a.AppointmentDate.Date == today),
                UpcomingAppointments = doctor.Appointments.Count(a =>
                    a.AppointmentDate.Date >= today &&
                    a.Status.Name != StatusNames.Completed &&
                    a.Status.Name != StatusNames.Cancelled &&
                    a.Status.Name != StatusNames.Missed),
                RequestedAppointments = doctor.Appointments.Count(a => a.Status.Name == StatusNames.Requested),
                ConfirmedAppointments = doctor.Appointments.Count(a => a.Status.Name == StatusNames.Confirmed),
                CheckedInAppointments = doctor.Appointments.Count(a => a.Status.Name == StatusNames.CheckedIn),
                InProgressAppointments = doctor.Appointments.Count(a => a.Status.Name == StatusNames.InProgress),
                CompletedAppointments = doctor.Appointments.Count(a => a.Status.Name == StatusNames.Completed),
                CancelledAppointments = doctor.Appointments.Count(a => a.Status.Name == StatusNames.Cancelled),
                MissedAppointments = doctor.Appointments.Count(a => a.Status.Name == StatusNames.Missed),
                IsOnLeaveToday = IsDoctorOnLeaveToday(doctor.Leaves)
            };
        }

        public async Task<DoctorCreateViewModel> GetCreateDoctorViewModelAsync()
        {
            return new DoctorCreateViewModel
            {
                IsActive = true,
                SpecializationOptions = await GetSpecializationOptionsAsync()
            };
        }

        public async Task<(bool Success, string Message, int? DoctorId)> CreateDoctorAsync(DoctorCreateViewModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email.Trim()))
            {
                return (false, "Email already exists.", null);
            }

            if (await _context.Doctors.AnyAsync(d => d.LicenseNumber == model.LicenseNumber.Trim()))
            {
                return (false, "License number already exists.", null);
            }

            if (!await _roleManager.RoleExistsAsync("Doctor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Doctor"));
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName.Trim(),
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var userResult = await _userManager.CreateAsync(user, model.Password);

            if (!userResult.Succeeded)
            {
                return (false, string.Join(" ", userResult.Errors.Select(e => e.Description)), null);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Doctor");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return (false, string.Join(" ", roleResult.Errors.Select(e => e.Description)), null);
            }

            var doctor = new Doctor
            {
                UserId = user.Id,
                LicenseNumber = model.LicenseNumber.Trim(),
                Bio = model.Bio?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            await UpdateDoctorSpecializationRecordsAsync(doctor.Id, model.SelectedSpecializationIds);

            return (true, "Doctor created successfully.", doctor.Id);
        }

        public async Task<DoctorEditViewModel?> GetEditDoctorViewModelAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return null;
            }

            return new DoctorEditViewModel
            {
                DoctorId = doctor.Id,
                UserId = doctor.UserId,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? string.Empty,
                PhoneNumber = doctor.User.PhoneNumber,
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio,
                IsActive = doctor.User.IsActive,
                SelectedSpecializationIds = doctor.DoctorSpecializations
                    .Select(ds => ds.SpecializationId)
                    .ToList(),
                SpecializationOptions = await GetSpecializationOptionsAsync()
            };
        }

        public async Task<(bool Success, string Message)> EditDoctorAsync(DoctorEditViewModel model)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == model.DoctorId);

            if (doctor == null)
            {
                return (false, "Doctor was not found.");
            }

            var email = model.Email.Trim();

            if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != doctor.UserId))
            {
                return (false, "Email already exists.");
            }

            if (await _context.Doctors.AnyAsync(d =>
                    d.LicenseNumber == model.LicenseNumber.Trim() &&
                    d.Id != doctor.Id))
            {
                return (false, "License number already exists.");
            }

            doctor.User.FullName = model.FullName.Trim();
            doctor.User.Email = email;
            doctor.User.UserName = email;
            doctor.User.PhoneNumber = model.PhoneNumber;
            doctor.User.IsActive = model.IsActive;

            doctor.LicenseNumber = model.LicenseNumber.Trim();
            doctor.Bio = model.Bio?.Trim();
            doctor.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(doctor.User);
                var passwordResult = await _userManager.ResetPasswordAsync(doctor.User, token, model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    return (false, string.Join(" ", passwordResult.Errors.Select(e => e.Description)));
                }
            }

            await _context.SaveChangesAsync();
            await UpdateDoctorSpecializationRecordsAsync(doctor.Id, model.SelectedSpecializationIds);

            return (true, "Doctor updated successfully.");
        }

        // =========================
        // Doctor Schedule Management
        // =========================

        public async Task<ManageDoctorScheduleViewModel?> GetDoctorScheduleAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return null;
            }

            return new ManageDoctorScheduleViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                DoctorEmail = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                Schedules = doctor.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new DoctorScheduleItemViewModel
                    {
                        DoctorScheduleId = s.Id,
                        DoctorId = s.DoctorId,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        SlotDurationMinutes = s.SlotDurationMinutes
                    })
                    .ToList()
            };
        }

        public async Task<DoctorScheduleFormViewModel?> GetCreateScheduleViewModelAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return null;
            }

            return new DoctorScheduleFormViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                DayOfWeek = DayOfWeek.Sunday,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(15, 0),
                SlotDurationMinutes = 30
            };
        }

        public async Task<(bool Success, string Message)> CreateScheduleAsync(DoctorScheduleFormViewModel model)
        {
            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == model.DoctorId)
                .ToListAsync();

            var validationMessage = ValidateSchedule(model, schedules, null);

            if (validationMessage != null)
            {
                return (false, validationMessage);
            }

            var schedule = new DoctorSchedule
            {
                DoctorId = model.DoctorId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                SlotDurationMinutes = model.SlotDurationMinutes
            };

            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return (true, "Schedule created successfully. Please review impacted appointments.");
        }

        public async Task<DoctorScheduleFormViewModel?> GetEditScheduleViewModelAsync(int doctorScheduleId)
        {
            var schedule = await _context.DoctorSchedules
                .AsNoTracking()
                .Include(s => s.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(s => s.Id == doctorScheduleId);

            if (schedule == null)
            {
                return null;
            }

            return new DoctorScheduleFormViewModel
            {
                DoctorScheduleId = schedule.Id,
                DoctorId = schedule.DoctorId,
                DoctorName = schedule.Doctor.User.FullName,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                SlotDurationMinutes = schedule.SlotDurationMinutes
            };
        }

        public async Task<(bool Success, string Message, int? DoctorId)> EditScheduleAsync(DoctorScheduleFormViewModel model)
        {
            if (!model.DoctorScheduleId.HasValue)
            {
                return (false, "Schedule id is missing.", null);
            }

            var schedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.Id == model.DoctorScheduleId.Value);

            if (schedule == null)
            {
                return (false, "Schedule was not found.", null);
            }

            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == schedule.DoctorId)
                .ToListAsync();

            var validationMessage = ValidateSchedule(model, schedules, schedule.Id);

            if (validationMessage != null)
            {
                return (false, validationMessage, schedule.DoctorId);
            }

            schedule.DayOfWeek = model.DayOfWeek;
            schedule.StartTime = model.StartTime;
            schedule.EndTime = model.EndTime;
            schedule.SlotDurationMinutes = model.SlotDurationMinutes;

            await _context.SaveChangesAsync();

            return (true, "Schedule updated successfully. Please review impacted appointments.", schedule.DoctorId);
        }

        public async Task<(bool Success, string Message, int? DoctorId)> DeleteScheduleAsync(int doctorScheduleId)
        {
            var schedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.Id == doctorScheduleId);

            if (schedule == null)
            {
                return (false, "Schedule was not found.", null);
            }

            var doctorId = schedule.DoctorId;

            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return (true, "Schedule deleted successfully. Please review impacted appointments.", doctorId);
        }

        // =========================
        // Doctor Leave Management
        // =========================

        public async Task<ManageDoctorLeavesViewModel?> GetDoctorLeavesAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return null;
            }

            return new ManageDoctorLeavesViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                DoctorEmail = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                Leaves = doctor.Leaves
                    .OrderByDescending(l => l.StartDate)
                    .Select(l => new DoctorLeaveItemViewModel
                    {
                        DoctorLeaveId = l.Id,
                        DoctorId = l.DoctorId,
                        LeaveStartDate = l.StartDate,
                        LeaveEndDate = l.EndDate,
                        LeaveReason = l.Reason ?? string.Empty
                    })
                    .ToList()
            };
        }

        public async Task<DoctorLeaveFormViewModel?> GetCreateLeaveViewModelAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return null;
            }

            return new DoctorLeaveFormViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                LeaveStartDate = DateTime.Today,
                LeaveEndDate = DateTime.Today
            };
        }

        public async Task<(bool Success, string Message)> CreateLeaveAsync(DoctorLeaveFormViewModel model)
        {
            var leaves = await _context.DoctorLeaves
                .Where(l => l.DoctorId == model.DoctorId)
                .ToListAsync();

            var validationMessage = ValidateLeave(model, leaves, null);

            if (validationMessage != null)
            {
                return (false, validationMessage);
            }

            var leave = new DoctorLeave
            {
                DoctorId = model.DoctorId,
                StartDate = model.LeaveStartDate.Date,
                EndDate = model.LeaveEndDate.Date,
                Reason = model.LeaveReason.Trim()
            };

            _context.DoctorLeaves.Add(leave);
            await _context.SaveChangesAsync();

            return (true, "Doctor leave created successfully. Please review impacted appointments.");
        }

        public async Task<DoctorLeaveFormViewModel?> GetEditLeaveViewModelAsync(int doctorLeaveId)
        {
            var leave = await _context.DoctorLeaves
                .AsNoTracking()
                .Include(l => l.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(l => l.Id == doctorLeaveId);

            if (leave == null)
            {
                return null;
            }

            return new DoctorLeaveFormViewModel
            {
                DoctorLeaveId = leave.Id,
                DoctorId = leave.DoctorId,
                DoctorName = leave.Doctor.User.FullName,
                LeaveStartDate = leave.StartDate,
                LeaveEndDate = leave.EndDate,
                LeaveReason = leave.Reason ?? string.Empty
            };
        }

        public async Task<(bool Success, string Message, int? DoctorId)> EditLeaveAsync(DoctorLeaveFormViewModel model)
        {
            if (!model.DoctorLeaveId.HasValue)
            {
                return (false, "Leave id is missing.", null);
            }

            var leave = await _context.DoctorLeaves
                .FirstOrDefaultAsync(l => l.Id == model.DoctorLeaveId.Value);

            if (leave == null)
            {
                return (false, "Leave was not found.", null);
            }

            var leaves = await _context.DoctorLeaves
                .Where(l => l.DoctorId == leave.DoctorId)
                .ToListAsync();

            var validationMessage = ValidateLeave(model, leaves, leave.Id);

            if (validationMessage != null)
            {
                return (false, validationMessage, leave.DoctorId);
            }

            leave.StartDate = model.LeaveStartDate.Date;
            leave.EndDate = model.LeaveEndDate.Date;
            leave.Reason = model.LeaveReason.Trim();

            await _context.SaveChangesAsync();

            return (true, "Doctor leave updated successfully. Please review impacted appointments.", leave.DoctorId);
        }

        public async Task<(bool Success, string Message, int? DoctorId)> DeleteLeaveAsync(int doctorLeaveId)
        {
            var leave = await _context.DoctorLeaves
                .FirstOrDefaultAsync(l => l.Id == doctorLeaveId);

            if (leave == null)
            {
                return (false, "Leave was not found.", null);
            }

            var doctorId = leave.DoctorId;

            _context.DoctorLeaves.Remove(leave);
            await _context.SaveChangesAsync();

            return (true, "Doctor leave deleted successfully. Please review impacted appointments.", doctorId);
        }

        // =========================
        // Appointment Impact
        // =========================

        public async Task<AppointmentImpactViewModel?> GetAppointmentImpactAsync(
            int doctorId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return null;
            }

            var start = fromDate?.Date ?? DateTime.Today;
            var end = toDate?.Date ?? DateTime.Today.AddMonths(1);

            if (end < start)
            {
                end = start;
            }

            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date >= start &&
                    a.AppointmentDate.Date <= end &&
                    a.Status.Name != StatusNames.Completed &&
                    a.Status.Name != StatusNames.Cancelled &&
                    a.Status.Name != StatusNames.Missed)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var schedules = doctor.Schedules.ToList();
            var leaves = doctor.Leaves.ToList();

            var impacted = appointments
                .Where(a => IsAppointmentImpacted(a, schedules, leaves))
                .Select(a => new ImpactedAppointmentItemViewModel
                {
                    AppointmentId = a.Id,
                    DoctorId = a.DoctorId,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StatusName = _workflowService.FormatStatusName(a.Status.Name),
                    Notes = a.Notes ?? string.Empty,
                    ImpactType = GetImpactType(a, leaves),
                    ImpactReason = BuildImpactReason(a, schedules, leaves)
                })
                .ToList();

            return new AppointmentImpactViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                DoctorEmail = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                FromDate = start,
                ToDate = end,
                ImpactedAppointments = impacted
            };
        }

        public async Task<(bool Success, string Message, int? DoctorId)> CancelImpactedAppointmentAsync(
            int appointmentId,
            string? reason)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Status)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return (false, "Appointment was not found.", null);
            }

            if (!_workflowService.CanUpdateStatus(appointment.Status.Name))
            {
                return (false, "This appointment cannot be cancelled.", appointment.DoctorId);
            }

            var cancelledStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == StatusNames.Cancelled);

            if (cancelledStatus == null)
            {
                return (false, "Cancelled status was not found.", appointment.DoctorId);
            }

            appointment.StatusId = cancelledStatus.Id;
            appointment.CancellationReason = string.IsNullOrWhiteSpace(reason)
                ? "Cancelled by clinic manager due to doctor availability change."
                : reason.Trim();
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await NotifyAppointmentUsersAsync(
                appointment,
                "Appointment Cancelled",
                $"Appointment on {appointment.AppointmentDate:dd MMM yyyy} was cancelled due to doctor availability change.");

            return (true, "Appointment cancelled and notifications were created.", appointment.DoctorId);
        }

        // =========================
        // Appointments
        // =========================

        public async Task<ClinicManagerAppointmentsViewModel> GetAppointmentsAsync(
            int? doctorId,
            string? status,
            DateTime? date,
            string? searchTerm)
        {
            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .AsQueryable();

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = _workflowService.NormalizeStatusName(status);
                query = query.Where(a => a.Status.Name == normalizedStatus);
            }

            if (date.HasValue)
            {
                var selectedDate = date.Value.Date;
                query = query.Where(a => a.AppointmentDate.Date == selectedDate);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();

                query = query.Where(a =>
                    a.Patient.User.FullName.ToLower().Contains(term) ||
                    a.Doctor.User.FullName.ToLower().Contains(term) ||
                    a.Patient.ReferenceNumber.ToLower().Contains(term) ||
                    a.Patient.CPRNumber.ToLower().Contains(term));
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return new ClinicManagerAppointmentsViewModel
            {
                SelectedDoctorId = doctorId,
                SelectedStatus = status,
                SelectedDate = date,
                SearchTerm = searchTerm,
                DoctorOptions = await GetDoctorOptionsAsync(),
                StatusOptions = await GetStatusOptionsAsync(),

                Appointments = appointments.Select(a => new ClinicManagerAppointmentItemViewModel
                {
                    AppointmentId = a.Id,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    PatientReferenceNumber = a.Patient.ReferenceNumber,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StatusName = _workflowService.FormatStatusName(a.Status.Name),
                    Notes = a.Notes ?? string.Empty,
                    CancellationReason = a.CancellationReason ?? string.Empty
                }).ToList()
            };
        }

        public async Task<ClinicManagerAppointmentDetailsViewModel?> GetAppointmentDetailsAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                    .ThenInclude(v => v!.Prescriptions)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return null;
            }

            return new ClinicManagerAppointmentDetailsViewModel
            {
                AppointmentId = appointment.Id,

                PatientId = appointment.PatientId,
                PatientName = appointment.Patient.User.FullName,
                PatientEmail = appointment.Patient.User.Email ?? string.Empty,
                PatientPhoneNumber = appointment.Patient.User.PhoneNumber ?? string.Empty,
                PatientReferenceNumber = appointment.Patient.ReferenceNumber,
                PatientCprNumber = appointment.Patient.CPRNumber,

                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.User.FullName,
                DoctorEmail = appointment.Doctor.User.Email ?? string.Empty,
                DoctorLicenseNumber = appointment.Doctor.LicenseNumber,

                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                StatusName = _workflowService.FormatStatusName(appointment.Status.Name),
                Notes = appointment.Notes ?? string.Empty,
                CancellationReason = appointment.CancellationReason ?? string.Empty,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,

                HasVisitRecord = appointment.VisitRecord != null,
                VisitRecordId = appointment.VisitRecord?.Id,
                Diagnosis = appointment.VisitRecord?.Diagnosis ?? string.Empty,
                Treatment = appointment.VisitRecord?.Treatment ?? string.Empty,
                DoctorNotes = appointment.VisitRecord?.DoctorNotes ?? string.Empty,
                PrescriptionCount = appointment.VisitRecord?.Prescriptions.Count ?? 0
            };
        }

        public async Task<ClinicManagerAppointmentStatusViewModel?> GetAppointmentStatusViewModelAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return null;
            }

            var allowedStatuses = _workflowService.GetAllowedNextStatuses(appointment.Status.Name);

            return new ClinicManagerAppointmentStatusViewModel
            {
                AppointmentId = appointment.Id,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient.User.FullName,
                DoctorName = appointment.Doctor.User.FullName,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                CurrentStatusName = _workflowService.FormatStatusName(appointment.Status.Name),
                StatusOptions = allowedStatuses.Select(s => new SelectListItem
                {
                    Value = s,
                    Text = _workflowService.FormatStatusName(s)
                }).ToList()
            };
        }

        public async Task<(bool Success, string Message)> UpdateAppointmentStatusAsync(
            ClinicManagerAppointmentStatusViewModel model)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Status)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == model.AppointmentId);

            if (appointment == null)
            {
                return (false, "Appointment was not found.");
            }

            var newStatus = _workflowService.NormalizeStatusName(model.NewStatusName);

            if (!_workflowService.IsValidStatusTransition(appointment.Status.Name, newStatus))
            {
                return (false, "This status change is not allowed.");
            }

            if (newStatus == StatusNames.Cancelled && string.IsNullOrWhiteSpace(model.CancellationReason))
            {
                return (false, "Cancellation reason is required.");
            }

            var statusEntity = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == newStatus);

            if (statusEntity == null)
            {
                return (false, "Selected status was not found.");
            }

            appointment.StatusId = statusEntity.Id;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (newStatus == StatusNames.Cancelled)
            {
                appointment.CancellationReason = model.CancellationReason?.Trim();
            }

            await _context.SaveChangesAsync();

            await NotifyAppointmentUsersAsync(
                appointment,
                "Appointment Status Updated",
                $"Appointment on {appointment.AppointmentDate:dd MMM yyyy} is now {_workflowService.FormatStatusName(newStatus)}.");

            return (true, "Appointment status updated successfully.");
        }

        // =========================
        // Specializations
        // =========================

        public async Task<ManageDoctorSpecializationsViewModel?> GetDoctorSpecializationsAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return null;
            }

            var selectedIds = doctor.DoctorSpecializations
                .Select(ds => ds.SpecializationId)
                .ToHashSet();

            var specializations = await _context.Specializations
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();

            return new ManageDoctorSpecializationsViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                DoctorEmail = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                SelectedSpecializationIds = selectedIds.ToList(),

                Specializations = specializations.Select(s => new SpecializationSelectionViewModel
                {
                    SpecializationId = s.Id,
                    SpecializationName = s.Name,
                    Description = s.Description ?? string.Empty,
                    IsSelected = selectedIds.Contains(s.Id)
                }).ToList()
            };
        }

        public async Task<(bool Success, string Message)> UpdateDoctorSpecializationsAsync(
            ManageDoctorSpecializationsViewModel model)
        {
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == model.DoctorId);

            if (!doctorExists)
            {
                return (false, "Doctor was not found.");
            }

            await UpdateDoctorSpecializationRecordsAsync(model.DoctorId, model.SelectedSpecializationIds);

            return (true, "Doctor specializations updated successfully.");
        }

        // =========================
        // Reports
        // =========================

        public async Task<ClinicReportViewModel> GetReportsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var start = fromDate?.Date ?? DateTime.Today.AddDays(-30);
            var end = toDate?.Date ?? DateTime.Today;

            if (end < start)
            {
                end = start;
            }

            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Status)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.AppointmentDate.Date >= start && a.AppointmentDate.Date <= end)
                .ToListAsync();

            var doctors = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Leaves)
                .ToListAsync();

            var totalAppointments = appointments.Count;
            var completed = appointments.Count(a => a.Status.Name == StatusNames.Completed);
            var cancelled = appointments.Count(a => a.Status.Name == StatusNames.Cancelled);
            var missed = appointments.Count(a => a.Status.Name == StatusNames.Missed);

            return new ClinicReportViewModel
            {
                FromDate = start,
                ToDate = end,

                TotalAppointments = totalAppointments,
                RequestedAppointments = appointments.Count(a => a.Status.Name == StatusNames.Requested),
                ConfirmedAppointments = appointments.Count(a => a.Status.Name == StatusNames.Confirmed),
                CheckedInAppointments = appointments.Count(a => a.Status.Name == StatusNames.CheckedIn),
                InProgressAppointments = appointments.Count(a => a.Status.Name == StatusNames.InProgress),
                CompletedAppointments = completed,
                CancelledAppointments = cancelled,
                MissedAppointments = missed,

                TotalDoctors = doctors.Count,
                ActiveDoctors = doctors.Count(d => d.User.IsActive),
                DoctorsWithAppointments = appointments.Select(a => a.DoctorId).Distinct().Count(),
                DoctorsOnLeave = doctors.Count(d => d.Leaves.Any(l =>
                    l.StartDate.Date <= end && l.EndDate.Date >= start)),

                CompletionRate = CalculateRate(completed, totalAppointments),
                CancellationRate = CalculateRate(cancelled, totalAppointments),
                MissedRate = CalculateRate(missed, totalAppointments),

                DoctorReports = appointments
                    .GroupBy(a => new
                    {
                        a.DoctorId,
                        a.Doctor.User.FullName,
                        a.Doctor.User.Email,
                        a.Doctor.LicenseNumber
                    })
                    .Select(g =>
                    {
                        var doctorTotal = g.Count();
                        var doctorCompleted = g.Count(a => a.Status.Name == StatusNames.Completed);
                        var doctorCancelled = g.Count(a => a.Status.Name == StatusNames.Cancelled);
                        var doctorMissed = g.Count(a => a.Status.Name == StatusNames.Missed);

                        return new ClinicReportItemViewModel
                        {
                            DoctorId = g.Key.DoctorId,
                            DoctorName = g.Key.FullName,
                            DoctorEmail = g.Key.Email ?? string.Empty,
                            LicenseNumber = g.Key.LicenseNumber,
                            TotalAppointments = doctorTotal,
                            CompletedAppointments = doctorCompleted,
                            CancelledAppointments = doctorCancelled,
                            MissedAppointments = doctorMissed,
                            RemainingAppointments = doctorTotal - doctorCompleted - doctorCancelled - doctorMissed,
                            CompletionRate = CalculateRate(doctorCompleted, doctorTotal),
                            CancellationRate = CalculateRate(doctorCancelled, doctorTotal),
                            MissedRate = CalculateRate(doctorMissed, doctorTotal)
                        };
                    })
                    .OrderByDescending(r => r.TotalAppointments)
                    .ToList()
            };
        }

        // =========================
        // Notifications
        // =========================

        public async Task<ClinicManagerNotificationViewModel> GetNotificationsAsync(string userId)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            return new ClinicManagerNotificationViewModel
            {
                Notifications = notifications.Select(n => new ClinicManagerNotificationItemViewModel
                {
                    NotificationId = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    NotificationTypeName = n.NotificationType?.Name ?? "General",
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    RelatedEntityId = n.RelatedEntityId,
                    RelatedEntityType = n.RelatedEntityType ?? string.Empty
                }).ToList()
            };
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId)
        {
            return await _notificationService.MarkAsReadAsync(notificationId, userId);
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
        }

        // =========================
        // Private Helpers
        // =========================

        private async Task<List<SelectListItem>> GetSpecializationOptionsAsync()
        {
            return await _context.Specializations
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetDoctorOptionsAsync()
        {
            return await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .OrderBy(d => d.User.FullName)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.User.FullName
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetStatusOptionsAsync()
        {
            return await _context.AppointmentStatuses
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .Select(s => new SelectListItem
                {
                    Value = s.Name,
                    Text = _workflowService.FormatStatusName(s.Name)
                })
                .ToListAsync();
        }

        private async Task UpdateDoctorSpecializationRecordsAsync(int doctorId, List<int> selectedSpecializationIds)
        {
            var selectedIds = selectedSpecializationIds
                .Distinct()
                .ToList();

            var validIds = await _context.Specializations
                .Where(s => selectedIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            var oldRecords = await _context.DoctorSpecializations
                .Where(ds => ds.DoctorId == doctorId)
                .ToListAsync();

            _context.DoctorSpecializations.RemoveRange(oldRecords);

            foreach (var specializationId in validIds)
            {
                _context.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorId = doctorId,
                    SpecializationId = specializationId
                });
            }

            await _context.SaveChangesAsync();
        }

        private static string? ValidateSchedule(
            DoctorScheduleFormViewModel model,
            List<DoctorSchedule> existingSchedules,
            int? excludeScheduleId)
        {
            if (model.EndTime <= model.StartTime)
            {
                return "End time must be after start time.";
            }

            if (model.SlotDurationMinutes <= 0)
            {
                return "Slot duration must be greater than 0.";
            }

            var totalMinutes = (model.EndTime.ToTimeSpan() - model.StartTime.ToTimeSpan()).TotalMinutes;

            if (model.SlotDurationMinutes > totalMinutes)
            {
                return "Slot duration cannot be longer than the schedule time range.";
            }

            var hasOverlap = existingSchedules.Any(s =>
                (!excludeScheduleId.HasValue || s.Id != excludeScheduleId.Value) &&
                s.DayOfWeek == model.DayOfWeek &&
                model.StartTime < s.EndTime &&
                model.EndTime > s.StartTime);

            if (hasOverlap)
            {
                return "This schedule overlaps with an existing schedule for the same day.";
            }

            return null;
        }

        private static string? ValidateLeave(
            DoctorLeaveFormViewModel model,
            List<DoctorLeave> existingLeaves,
            int? excludeLeaveId)
        {
            if (model.LeaveEndDate.Date < model.LeaveStartDate.Date)
            {
                return "Leave end date cannot be before leave start date.";
            }

            if (string.IsNullOrWhiteSpace(model.LeaveReason))
            {
                return "Leave reason is required.";
            }

            var hasOverlap = existingLeaves.Any(l =>
                (!excludeLeaveId.HasValue || l.Id != excludeLeaveId.Value) &&
                model.LeaveStartDate.Date <= l.EndDate.Date &&
                model.LeaveEndDate.Date >= l.StartDate.Date);

            if (hasOverlap)
            {
                return "This leave period overlaps with an existing leave period.";
            }

            return null;
        }

        private static bool IsDoctorOnLeaveToday(IEnumerable<DoctorLeave> leaves)
        {
            var today = DateTime.Today;

            return leaves.Any(l =>
                l.StartDate.Date <= today &&
                l.EndDate.Date >= today);
        }

        private static bool IsAppointmentImpacted(
            Appointment appointment,
            List<DoctorSchedule> schedules,
            List<DoctorLeave> leaves)
        {
            var appointmentDate = appointment.AppointmentDate.Date;

            var isOnLeave = leaves.Any(l =>
                appointmentDate >= l.StartDate.Date &&
                appointmentDate <= l.EndDate.Date);

            if (isOnLeave)
            {
                return true;
            }

            var matchingSchedules = schedules
                .Where(s => s.DayOfWeek == appointment.AppointmentDate.DayOfWeek)
                .ToList();

            if (!matchingSchedules.Any())
            {
                return true;
            }

            var insideSchedule = matchingSchedules.Any(s =>
                appointment.StartTime >= s.StartTime &&
                appointment.EndTime <= s.EndTime);

            return !insideSchedule;
        }

        private static string GetImpactType(Appointment appointment, List<DoctorLeave> leaves)
        {
            var appointmentDate = appointment.AppointmentDate.Date;

            var isLeaveImpact = leaves.Any(l =>
                appointmentDate >= l.StartDate.Date &&
                appointmentDate <= l.EndDate.Date);

            return isLeaveImpact ? "Leave" : "Schedule";
        }

        private static string BuildImpactReason(
            Appointment appointment,
            List<DoctorSchedule> schedules,
            List<DoctorLeave> leaves)
        {
            var appointmentDate = appointment.AppointmentDate.Date;

            var leave = leaves.FirstOrDefault(l =>
                appointmentDate >= l.StartDate.Date &&
                appointmentDate <= l.EndDate.Date);

            if (leave != null)
            {
                return string.IsNullOrWhiteSpace(leave.Reason)
                    ? "Doctor is on leave on this date."
                    : $"Doctor is on leave on this date: {leave.Reason}";
            }

            var matchingSchedules = schedules
                .Where(s => s.DayOfWeek == appointment.AppointmentDate.DayOfWeek)
                .ToList();

            if (!matchingSchedules.Any())
            {
                return "Doctor has no working schedule for this day.";
            }

            return "Appointment time is outside the current doctor schedule.";
        }

        private async Task NotifyAppointmentUsersAsync(Appointment appointment, string title, string message)
        {
            var userIds = new List<string>
            {
                appointment.Patient.UserId,
                appointment.Doctor.UserId
            };

            await _notificationService.CreateNotificationsAsync(
                userIds,
                title,
                message,
                "Appointment",
                appointment.Id,
                "Appointment");
        }

        private static double CalculateRate(int value, int total)
        {
            if (total == 0)
            {
                return 0;
            }

            return Math.Round((double)value / total * 100, 1);
        }

        public async Task<ClinicManagerProfileViewModel?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            return new ClinicManagerProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                ProfilePicture = user.ProfilePicture
            };
        }

        public async Task<EditClinicManagerProfileViewModel?> GetEditProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            return new EditClinicManagerProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                CurrentProfilePicture = user.ProfilePicture
            };
        }

        public async Task<bool> UpdateProfileAsync(
            string userId,
            EditClinicManagerProfileViewModel model,
            string webRootPath)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            await _userManager.SetEmailAsync(user, model.Email);
            await _userManager.SetUserNameAsync(user, model.Email);

            if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
            {
                var extension = Path.GetExtension(model.ProfilePictureFile.FileName).ToLower();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    return false;
                }

                var uploadsFolder = Path.Combine(webRootPath, "images", "managers");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"manager-{user.Id}-{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePictureFile.CopyToAsync(stream);
                }

                user.ProfilePicture = fileName;
            }

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
    }
}