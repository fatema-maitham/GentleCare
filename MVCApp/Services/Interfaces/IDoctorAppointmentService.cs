using MVCApp.ViewModels.Doctor;

namespace MVCApp.Services.Interfaces
{
    // Service contract for doctor appointment list, details, patient history, status updates,
    // and follow-up appointment requests.
    public interface IDoctorAppointmentService
    {
        Task<DoctorAppointmentListViewModel?> GetAppointmentsAsync(
            string userId,
            string? searchTerm,
            string? status,
            DateTime? date);

        Task<DoctorAppointmentDetailsViewModel?> GetAppointmentDetailsAsync(
            string userId,
            int appointmentId);

        Task<UpdateAppointmentStatusViewModel?> GetUpdateStatusModelAsync(
            string userId,
            int appointmentId);

        Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(
            string userId,
            UpdateAppointmentStatusViewModel model);

        Task<DoctorPatientHistoryViewModel?> GetPatientHistoryAsync(
            string userId,
            int patientId);

        Task<CreateFollowUpRequestViewModel?> GetCreateFollowUpRequestAsync(
            string userId,
            int appointmentId);

        Task<(bool Success, string? ErrorMessage, int? NewAppointmentId)> CreateFollowUpRequestAsync(
            string userId,
            CreateFollowUpRequestViewModel model);
    }
}