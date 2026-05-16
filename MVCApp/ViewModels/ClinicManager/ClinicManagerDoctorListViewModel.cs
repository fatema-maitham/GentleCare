namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel for the Clinic Manager doctors list page.
    // It contains search/filter values and the list of doctors.
    public class ClinicManagerDoctorListViewModel
    {
        // Search box value
        public string? SearchTerm { get; set; }

        // Optional filter:
        // true = active doctors only
        // false = inactive doctors only
        // null = all doctors
        public bool? IsActive { get; set; }

        // Doctors displayed on the page
        public List<ClinicManagerDoctorListItemViewModel> Doctors { get; set; } = new();

        // Summary values
        public int TotalDoctors => Doctors.Count;

        public int ActiveDoctors => Doctors.Count(d => d.IsActive);

        public int InactiveDoctors => Doctors.Count(d => !d.IsActive);

        public int DoctorsOnLeaveToday => Doctors.Count(d => d.IsOnLeaveToday);

        public int DoctorsWithAppointmentsToday => Doctors.Count(d => d.TodayAppointmentsCount > 0);

        public int DoctorsWithUpcomingAppointments => Doctors.Count(d => d.UpcomingAppointmentsCount > 0);

        // View helper properties
        public bool HasDoctors => Doctors.Any();

        public bool HasSearch =>
            !string.IsNullOrWhiteSpace(SearchTerm) || IsActive.HasValue;

        public string EmptyMessage =>
            HasSearch
                ? "No doctors match the selected search or filter."
                : "No doctors have been added yet.";
    }
}