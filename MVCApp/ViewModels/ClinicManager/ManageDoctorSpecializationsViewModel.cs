namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager to manage one doctor's specializations.
    public class ManageDoctorSpecializationsViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string DoctorEmail { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        // Selected specialization ids from the form.
        public List<int> SelectedSpecializationIds { get; set; } = new();

        // All specialization options shown as checkboxes.
        public List<SpecializationSelectionViewModel> Specializations { get; set; } = new();

        // View helper properties
        public bool HasSpecializations => Specializations.Any();

        public int TotalSpecializations => Specializations.Count;

        public int SelectedCount => SelectedSpecializationIds.Count;

        public string EmptyMessage => "No specializations are available yet.";
    }
}