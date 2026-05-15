using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used by the Doctor Appointments page.
    // It contains filter/search values and the appointment list shown to the logged-in doctor.
    public class DoctorAppointmentListViewModel
    {
        // Doctor name displayed at the top of the appointments page.
        public string DoctorFullName { get; set; } = string.Empty;

        // Search by patient name, CPR, reference number, or notes.
        public string? SearchTerm { get; set; }

        // Selected appointment status filter, for example Confirmed or Completed.
        public string? SelectedStatus { get; set; }

        // Selected appointment date filter.
        public DateTime? SelectedDate { get; set; }

        // Dropdown options for appointment statuses.
        public List<SelectListItem> StatusOptions { get; set; } = new();

        // Appointments assigned to the logged-in doctor.
        public List<DoctorAppointmentListItemViewModel> Appointments { get; set; } = new();

        // Helpful computed value for showing whether the doctor has appointments after filtering.
        public bool HasAppointments => Appointments.Any();

        // Total number of appointments after applying filters.
        public int TotalAppointments => Appointments.Count;

        // Used by the view to show filter summary / clear button meaningfully.
        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(SearchTerm) ||
            !string.IsNullOrWhiteSpace(SelectedStatus) ||
            SelectedDate.HasValue;
    }
}