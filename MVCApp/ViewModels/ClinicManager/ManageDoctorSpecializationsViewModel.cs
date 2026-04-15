namespace MVCApp.ViewModels.ClinicManager
{
    public class ManageDoctorSpecializationsViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public List<int> SelectedSpecializationIds { get; set; } = new();
        public List<SpecializationSelectionViewModel> Specializations { get; set; } = new();
    }

    public class SpecializationSelectionViewModel
    {
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
