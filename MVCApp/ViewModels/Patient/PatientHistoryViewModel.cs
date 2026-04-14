namespace MVCApp.ViewModels.Patient
{
    public class PatientHistoryViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Treatment { get; set; }
        public string DoctorNotes { get; set; } = string.Empty;
        public List<PrescriptionItemViewModel> Prescriptions { get; set; } = new();
    }
}