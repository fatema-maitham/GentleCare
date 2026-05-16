namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistPatientSearchResultViewModel
    {
        public int PatientId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string CPRNumber { get; set; } = string.Empty;

        public string ReferenceNumber { get; set; } = string.Empty;

        public string DateOfBirth { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}