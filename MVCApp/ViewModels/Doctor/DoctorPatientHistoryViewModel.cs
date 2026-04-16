using System.ComponentModel.DataAnnotations;

namespace MVCApp.ViewModels.Doctor
{
    public class DoctorPatientHistoryViewModel
    {
        public int PatientId { get; set; }

        public string PatientFullName { get; set; } = string.Empty;
        public string CPRNumber { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        public string? BloodType { get; set; }
        public string? Address { get; set; }

        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Phone")]
        public string? EmergencyContactPhone { get; set; }

        public List<DoctorPatientVisitItemViewModel> Visits { get; set; } = new();
    }

    public class DoctorPatientVisitItemViewModel
    {
        public int AppointmentId { get; set; }

        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Start Time")]
        public TimeOnly StartTime { get; set; }

        [Display(Name = "End Time")]
        public TimeOnly EndTime { get; set; }

        public string StatusName { get; set; } = string.Empty;
        public string DoctorNotes { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Treatment { get; set; }
        public int PrescriptionCount { get; set; }
    }
}