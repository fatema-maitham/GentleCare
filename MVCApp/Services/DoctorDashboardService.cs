using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Doctor;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Services
{
    // Business logic for doctor dashboard-related pages.
    // The controller only calls this service; database logic stays here.
    public class DoctorDashboardService : IDoctorDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAppointmentWorkflowService _workflowService;

        public DoctorDashboardService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IAppointmentWorkflowService workflowService)
        {
            _context = context;
            _notificationService = notificationService;
            _workflowService = workflowService;
        }

        // Builds the doctor dashboard with statistics, calendar days, and selected-day appointments.
        public async Task<DoctorDashboardViewModel?> GetDashboardAsync(string userId, DateTime? selectedDate)
        {
            var doctor = await GetCurrentDoctorAsync(userId, includeUser: true);
            if (doctor == null)
            {
                return null;
            }

            var chosenDate = (selectedDate ?? DateTime.Today).Date;
            var firstDayOfMonth = new DateTime(chosenDate.Year, chosenDate.Month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            var monthAppointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.AppointmentDate >= firstDayOfMonth &&
                    a.AppointmentDate < firstDayOfNextMonth)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var selectedDayAppointments = monthAppointments
                .Where(a => a.AppointmentDate.Date == chosenDate)
                .OrderBy(a => a.StartTime)
                .ToList();

            var upcomingAppointmentsCount = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.AppointmentDate.Date >= DateTime.Today &&
                    a.Status.Name != "Completed" &&
                    a.Status.Name != "Cancelled" &&
                    a.Status.Name != "Missed")
                .CountAsync();

            var unreadNotificationsCount = await _notificationService.GetUnreadCountAsync(doctor.UserId);

            var totalPatientsSeen = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.Status.Name == "Completed")
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            return new DoctorDashboardViewModel
            {
                DoctorFullName = doctor.User.FullName,
                SelectedDate = chosenDate,
                TotalAppointmentsForSelectedDate = selectedDayAppointments.Count,
                ConfirmedAppointmentsForSelectedDate = selectedDayAppointments.Count(a => a.Status.Name == "Confirmed"),
                CheckedInAppointmentsForSelectedDate = selectedDayAppointments.Count(a => a.Status.Name == "CheckedIn"),
                InProgressAppointmentsForSelectedDate = selectedDayAppointments.Count(a => a.Status.Name == "InProgress"),
                CompletedAppointmentsForSelectedDate = selectedDayAppointments.Count(a => a.Status.Name == "Completed"),
                UpcomingAppointmentsCount = upcomingAppointmentsCount,
                UnreadNotificationsCount = unreadNotificationsCount,
                TotalPatientsSeen = totalPatientsSeen,
                CalendarDays = BuildCalendarDays(chosenDate, monthAppointments),
                SelectedDayAppointments = selectedDayAppointments.Select(a => new DoctorDashboardAppointmentItemViewModel
                {
                    AppointmentId = a.Id,
                    PatientFullName = a.Patient.User.FullName,
                    PatientReferenceNumber = a.Patient.ReferenceNumber,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StatusName = _workflowService.FormatStatusName(a.Status.Name),
                    Notes = a.Notes
                }).ToList()
            };
        }

        // Gets the logged-in doctor's profile information.
        public async Task<DoctorProfileViewModel?> GetProfileAsync(string userId)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.User.IsActive);

            if (doctor == null)
            {
                return null;
            }

            return new DoctorProfileViewModel
            {
                DoctorId = doctor.Id,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio,
                Specializations = doctor.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name)
                    .OrderBy(name => name)
                    .ToList()
            };
        }

        // Gets the logged-in doctor's weekly schedule and leave periods.
        public async Task<DoctorScheduleViewModel?> GetScheduleAsync(string userId)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.User.IsActive);

            if (doctor == null)
            {
                return null;
            }

            return new DoctorScheduleViewModel
            {
                DoctorFullName = doctor.User.FullName,
                Schedules = doctor.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .ToList(),
                Leaves = doctor.Leaves
                    .OrderByDescending(l => l.StartDate)
                    .ToList()
            };
        }

        // Gets all notifications for the logged-in doctor.
        public async Task<DoctorNotificationsViewModel?> GetNotificationsAsync(string userId)
        {
            var doctor = await GetCurrentDoctorAsync(userId, includeUser: false);
            if (doctor == null)
            {
                return null;
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(doctor.UserId);

            return new DoctorNotificationsViewModel
            {
                UnreadCount = notifications.Count(n => !n.IsRead),
                Notifications = notifications.Select(n => new DoctorNotificationViewModel
                {
                    NotificationId = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    RelatedEntityId = n.RelatedEntityId,
                    RelatedEntityType = n.RelatedEntityType
                }).ToList()
            };
        }

        // Marks one doctor notification as read.
        public async Task<bool> MarkNotificationAsReadAsync(string userId, int notificationId)
        {
            var doctor = await GetCurrentDoctorAsync(userId, includeUser: false);
            if (doctor == null)
            {
                return false;
            }

            return await _notificationService.MarkAsReadAsync(notificationId, doctor.UserId);
        }

        // Marks all doctor notifications as read.
        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            var doctor = await GetCurrentDoctorAsync(userId, includeUser: false);
            if (doctor == null)
            {
                return;
            }

            await _notificationService.MarkAllAsReadAsync(doctor.UserId);
        }

        // Finds the Doctor row linked to the logged-in Identity user.
        private async Task<Doctor?> GetCurrentDoctorAsync(string userId, bool includeUser)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            var query = _context.Doctors.AsQueryable();

            if (includeUser)
            {
                query = query.Include(d => d.User);
            }

            return await query.FirstOrDefaultAsync(d => d.UserId == userId && d.User.IsActive);
        }

        // Builds calendar boxes for the selected month, including previous/next month filler days.
        private static List<DoctorDashboardCalendarDayViewModel> BuildCalendarDays(
            DateTime selectedDate,
            List<Appointment> monthAppointments)
        {
            var firstDayOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var startOffset = (int)firstDayOfMonth.DayOfWeek;
            var endOffset = 6 - (int)lastDayOfMonth.DayOfWeek;

            var calendarStart = firstDayOfMonth.AddDays(-startOffset);
            var calendarEnd = lastDayOfMonth.AddDays(endOffset);

            var appointmentCounts = monthAppointments
                .GroupBy(a => a.AppointmentDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var days = new List<DoctorDashboardCalendarDayViewModel>();

            for (var date = calendarStart; date <= calendarEnd; date = date.AddDays(1))
            {
                appointmentCounts.TryGetValue(date.Date, out var count);

                days.Add(new DoctorDashboardCalendarDayViewModel
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = date.Month == selectedDate.Month,
                    IsSelected = date.Date == selectedDate.Date,
                    HasAppointments = count > 0,
                    AppointmentCount = count
                });
            }

            return days;
        }

        public async Task<EditDoctorProfileViewModel?> GetEditProfileAsync(string userId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.User.IsActive);

            if (doctor == null)
                return null;

            return new EditDoctorProfileViewModel
            {
                DoctorId = doctor.Id,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio,
                CurrentProfilePicture = doctor.User.ProfilePicture
            };
        }

        public async Task<bool> UpdateProfileAsync(
            string userId,
            EditDoctorProfileViewModel model,
            string webRootPath)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.User.IsActive);

            if (doctor == null)
                return false;

            doctor.User.FullName = model.FullName.Trim();
            doctor.User.Email = model.Email.Trim();
            doctor.User.UserName = model.Email.Trim();
            doctor.User.NormalizedEmail = model.Email.Trim().ToUpper();
            doctor.User.NormalizedUserName = model.Email.Trim().ToUpper();

            doctor.LicenseNumber = model.LicenseNumber.Trim();
            doctor.Bio = model.Bio?.Trim();
            doctor.UpdatedAt = DateTime.UtcNow;

            if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.ProfilePictureFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return false;

                var folderPath = Path.Combine(webRootPath, "images", "doctors");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = $"doctor-{doctor.Id}-{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ProfilePictureFile.CopyToAsync(stream);

                doctor.User.ProfilePicture = $"/images/doctors/{fileName}";
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
