using MVCApp.ViewModels.Doctor;

namespace MVCApp.Services.Interfaces
{
    public interface IDoctorDashboardService
    {
        Task<DoctorDashboardViewModel?> GetDashboardAsync(string userId, DateTime? selectedDate);
        Task<DoctorProfileViewModel?> GetProfileAsync(string userId);
        Task<DoctorScheduleViewModel?> GetScheduleAsync(string userId);
        Task<DoctorNotificationsViewModel?> GetNotificationsAsync(string userId);

        Task<bool> MarkNotificationAsReadAsync(string userId, int notificationId);
        Task MarkAllNotificationsAsReadAsync(string userId);

        Task<EditDoctorProfileViewModel?> GetEditProfileAsync(string userId);

        Task<bool> UpdateProfileAsync(
            string userId,
            EditDoctorProfileViewModel model,
            string webRootPath);
    }
}