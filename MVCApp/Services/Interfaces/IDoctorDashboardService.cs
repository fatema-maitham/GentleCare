using MVCApp.ViewModels.Doctor;

namespace MVCApp.Services.Interfaces
{
    // Service contract for doctor dashboard, profile, schedule, and notifications.
    public interface IDoctorDashboardService
    {
        Task<DoctorDashboardViewModel?> GetDashboardAsync(string userId, DateTime? selectedDate);
        Task<DoctorProfileViewModel?> GetProfileAsync(string userId);
        Task<DoctorScheduleViewModel?> GetScheduleAsync(string userId);
        Task<DoctorNotificationsViewModel?> GetNotificationsAsync(string userId);
        Task<bool> MarkNotificationAsReadAsync(string userId, int notificationId);
        Task MarkAllNotificationsAsReadAsync(string userId);
    }
}
