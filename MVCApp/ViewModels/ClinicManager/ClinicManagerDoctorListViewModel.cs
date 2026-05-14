namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel for the Clinic Manager doctors list page.
    // It contains search/filter values and the list of doctors.
    public class ClinicManagerDoctorListViewModel
    {
        // Search box value
        public string? SearchTerm { get; set; }

        // Optional filter
        public bool? IsActive { get; set; }

        // Doctors shown in the table/cards
        public List<ClinicManagerDoctorListItemViewModel> Doctors { get; set; } = new();

        // Helper properties for the view
        public int TotalDoctors => Doctors.Count;

        public int ActiveDoctors => Doctors.Count(d => d.IsActive);

        public int InactiveDoctors => Doctors.Count(d => !d.IsActive);

        public bool HasDoctors => Doctors.Any();
    }
}