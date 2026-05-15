using MVCApp.ViewModels.ClinicManager;

namespace MVCApp.Services.Interfaces
{
    // Service interface for all Clinic Manager business logic.
    // The ClinicManagerController will call these methods instead of writing database logic directly inside the controller.
    public interface IClinicManagerService
    {
        // =========================
        // Dashboard
        // =========================

        // Gets summary numbers, today's appointments, doctor workload, and doctors on leave.
        Task<ClinicManagerDashboardViewModel> GetDashboardAsync();


        // =========================
        // Doctor Management
        // =========================

        // Gets all doctors with search and active/inactive filter.
        Task<ClinicManagerDoctorListViewModel> GetDoctorsAsync(string? searchTerm, bool? isActive);

        // Gets full details for one doctor.
        Task<ClinicManagerDoctorDetailsViewModel?> GetDoctorDetailsAsync(int doctorId);

        // Prepares the create doctor form with specialization options.
        Task<DoctorCreateViewModel> GetCreateDoctorViewModelAsync();

        // Creates a new Identity user with Doctor role and creates the linked Doctor profile.
        Task<(bool Success, string Message, int? DoctorId)> CreateDoctorAsync(DoctorCreateViewModel model);

        // Prepares the edit doctor form with existing doctor data and specialization options.
        Task<DoctorEditViewModel?> GetEditDoctorViewModelAsync(int doctorId);

        // Updates doctor account/profile data and specializations.
        Task<(bool Success, string Message)> EditDoctorAsync(DoctorEditViewModel model);


        // =========================
        // Doctor Schedule Management
        // =========================

        // Gets one doctor's weekly schedule list.
        Task<ManageDoctorScheduleViewModel?> GetDoctorScheduleAsync(int doctorId);

        // Prepares create schedule form.
        Task<DoctorScheduleFormViewModel?> GetCreateScheduleViewModelAsync(int doctorId);

        // Adds a new schedule row for a doctor.
        Task<(bool Success, string Message)> CreateScheduleAsync(DoctorScheduleFormViewModel model);

        // Prepares edit schedule form.
        Task<DoctorScheduleFormViewModel?> GetEditScheduleViewModelAsync(int doctorScheduleId);

        // Updates an existing schedule row.
        Task<(bool Success, string Message, int? DoctorId)> EditScheduleAsync(DoctorScheduleFormViewModel model);

        // Deletes a schedule row.
        Task<(bool Success, string Message, int? DoctorId)> DeleteScheduleAsync(int doctorScheduleId);


        // =========================
        // Doctor Leave Management
        // =========================

        // Gets one doctor's leave periods.
        Task<ManageDoctorLeavesViewModel?> GetDoctorLeavesAsync(int doctorId);

        // Prepares create leave form.
        Task<DoctorLeaveFormViewModel?> GetCreateLeaveViewModelAsync(int doctorId);

        // Adds a new doctor leave period.
        Task<(bool Success, string Message)> CreateLeaveAsync(DoctorLeaveFormViewModel model);

        // Prepares edit leave form.
        Task<DoctorLeaveFormViewModel?> GetEditLeaveViewModelAsync(int doctorLeaveId);

        // Updates an existing doctor leave period.
        Task<(bool Success, string Message, int? DoctorId)> EditLeaveAsync(DoctorLeaveFormViewModel model);

        // Deletes a doctor leave period.
        Task<(bool Success, string Message, int? DoctorId)> DeleteLeaveAsync(int doctorLeaveId);


        // =========================
        // Appointment Impact
        // =========================

        // Finds appointments affected by doctor schedule or leave changes.
        Task<AppointmentImpactViewModel?> GetAppointmentImpactAsync(
            int doctorId,
            DateTime? fromDate,
            DateTime? toDate);

        // Cancels an appointment affected by schedule/leave changes and creates notifications.
        Task<(bool Success, string Message, int? DoctorId)> CancelImpactedAppointmentAsync(
            int appointmentId,
            string? reason);


        // =========================
        // Clinic Appointment Management
        // =========================

        // Gets all clinic appointments with filters.
        Task<ClinicManagerAppointmentsViewModel> GetAppointmentsAsync(
            int? doctorId,
            string? status,
            DateTime? date,
            string? searchTerm);

        // Gets full appointment details.
        Task<ClinicManagerAppointmentDetailsViewModel?> GetAppointmentDetailsAsync(int appointmentId);

        // Prepares appointment status update form with valid next statuses.
        Task<ClinicManagerAppointmentStatusViewModel?> GetAppointmentStatusViewModelAsync(int appointmentId);

        // Updates appointment status using workflow rules and creates notifications.
        Task<(bool Success, string Message)> UpdateAppointmentStatusAsync(
            ClinicManagerAppointmentStatusViewModel model);


        // =========================
        // Doctor Specializations
        // =========================

        // Gets specialization checkbox list for one doctor.
        Task<ManageDoctorSpecializationsViewModel?> GetDoctorSpecializationsAsync(int doctorId);

        // Saves selected specializations for one doctor.
        Task<(bool Success, string Message)> UpdateDoctorSpecializationsAsync(
            ManageDoctorSpecializationsViewModel model);


        // =========================
        // Reports
        // =========================

        // Gets clinic report statistics for selected date range.
        Task<ClinicReportViewModel> GetReportsAsync(DateTime? fromDate, DateTime? toDate);


        // =========================
        // Notifications
        // =========================

        // Gets notifications for the logged-in Clinic Manager user.
        Task<ClinicManagerNotificationViewModel> GetNotificationsAsync(string userId);

        // Marks one notification as read.
        Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId);

        // Marks all notifications as read.
        Task MarkAllNotificationsAsReadAsync(string userId);
    }
}