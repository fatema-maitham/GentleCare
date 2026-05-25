using MVCApp.ViewModels.Receptionist;
using System.Security.Claims;

namespace MVCApp.Services.Interfaces
{
    public interface IReceptionistService
    {
        Task<ReceptionistDashboardViewModel> GetDashboardAsync();

        Task<ReceptionistProfileViewModel?> GetProfileAsync(ClaimsPrincipal user);

        Task<ReceptionistEditProfileViewModel?> GetEditProfileAsync(ClaimsPrincipal user);

        Task<(bool Success, string Message)> UpdateProfileAsync(
            ClaimsPrincipal user,
            ReceptionistEditProfileViewModel model);

        Task<ReceptionistAppointmentsPageViewModel> GetAppointmentsAsync(
            string? searchText,
            DateTime? selectedDate,
            string? selectedStatus);

        Task<ReceptionistBookAppointmentViewModel> GetBookAppointmentModelAsync(
            int? patientId,
            int? specializationId,
            int? doctorId,
            DateTime? appointmentDate);

        Task<(bool Success, string Message)> BookAppointmentAsync(
            ReceptionistBookAppointmentViewModel model);

        Task<ReceptionistUpdateAppointmentStatusViewModel?> GetUpdateStatusModelAsync(
            int appointmentId);

        Task<(bool Success, string Message, ReceptionistUpdateAppointmentStatusViewModel? Model)> UpdateStatusAsync(
            ReceptionistUpdateAppointmentStatusViewModel model);

        Task<ReceptionistPatientSearchViewModel> SearchPatientsAsync(
            string? searchText);

        Task<ReceptionistLiveQueueViewModel> GetLiveQueueAsync();

        Task<(bool Success, string Message)> UpdateQueueStatusAsync(
            int appointmentId,
            string newStatus);
    }
}