using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to view and filter all clinic appointments.
    public class ClinicManagerAppointmentsViewModel
    {
        // Filter values
        public int? SelectedDoctorId { get; set; }

        public string? SelectedStatus { get; set; }

        public DateTime? SelectedDate { get; set; }

        public string? SearchTerm { get; set; }

        // Dropdown options
        public List<SelectListItem> DoctorOptions { get; set; } = new();

        public List<SelectListItem> StatusOptions { get; set; } = new();

        // Appointment list
        public List<ClinicManagerAppointmentItemViewModel> Appointments { get; set; } = new();

        // Summary values
        public int TotalAppointments => Appointments.Count;

        public int RequestedAppointments =>
            Appointments.Count(a => a.StatusName == "Requested");

        public int ConfirmedAppointments =>
            Appointments.Count(a => a.StatusName == "Confirmed");

        public int CheckedInAppointments =>
            Appointments.Count(a => a.StatusName == "Checked In" || a.StatusName == "CheckedIn");

        public int InProgressAppointments =>
            Appointments.Count(a => a.StatusName == "In Progress" || a.StatusName == "InProgress");

        public int CompletedAppointments =>
            Appointments.Count(a => a.StatusName == "Completed");

        public int CancelledAppointments =>
            Appointments.Count(a => a.StatusName == "Cancelled");

        public int MissedAppointments =>
            Appointments.Count(a => a.StatusName == "Missed");

        // View helper properties
        public bool HasAppointments => Appointments.Any();

        public bool HasFilters =>
            SelectedDoctorId.HasValue ||
            !string.IsNullOrWhiteSpace(SelectedStatus) ||
            SelectedDate.HasValue ||
            !string.IsNullOrWhiteSpace(SearchTerm);
    }
}