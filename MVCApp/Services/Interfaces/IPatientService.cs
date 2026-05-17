using MVCApp.ViewModels.Patient;
using System.Security.Claims;

namespace MVCApp.Services.Interfaces
{
    public interface IPatientService
    {
        Task<PatientDashboardViewModel?> GetDashboardAsync(ClaimsPrincipal user);

        Task<PatientProfileViewModel?> GetProfileAsync(ClaimsPrincipal user);

        Task<PatientEditProfileViewModel?> GetEditProfileAsync(ClaimsPrincipal user);

        Task<(bool Success, string Message)> UpdateProfileAsync(
            ClaimsPrincipal user,
            PatientEditProfileViewModel model);

        Task<List<PatientAppointmentsViewModel>> GetAppointmentsAsync(ClaimsPrincipal user);

        Task<List<PatientHistoryViewModel>> GetHistoryAsync(ClaimsPrincipal user);

        Task<List<PatientNotificationViewModel>> GetNotificationsAsync(ClaimsPrincipal user);

        Task MarkAllNotificationsAsReadAsync(ClaimsPrincipal user);

        Task<PatientBookAppointmentViewModel?> GetBookAppointmentModelAsync(
            ClaimsPrincipal user,
            int? specializationId = null,
            int? doctorId = null,
            DateTime? appointmentDate = null);

        Task<(bool Success, string Message)> BookAppointmentAsync(
            ClaimsPrincipal user,
            PatientBookAppointmentViewModel model);

        Task<(bool Success, string Message)> CancelAppointmentAsync(
            ClaimsPrincipal user,
            int appointmentId);
    }
}