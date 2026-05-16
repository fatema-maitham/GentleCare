namespace MVCApp.ViewModels.ClinicManager
{
    // One specialization option shown in the Clinic Manager specialization selection page.
    public class SpecializationSelectionViewModel
    {
        public int SpecializationId { get; set; }

        public string SpecializationName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsSelected { get; set; }
    }
}