using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.Doctor
{
    public class DoctorAppointmentDetailsViewModel
    {
        public int AppointmentId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string StatusName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }

        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string PatientReferenceNumber { get; set; } = string.Empty;
        public string PatientCprNumber { get; set; } = string.Empty;

        public string? DoctorNotes { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }

        public bool HasVisitRecord { get; set; }
        public bool CanCreateVisitRecord { get; set; }
        public bool CanEditVisitRecord { get; set; }
        public bool CanUpdateStatus { get; set; }

        public List<SelectListItem> AvailableNextStatuses { get; set; } = new();
        public List<PrescriptionInputViewModel> Prescriptions { get; set; } = new();
    }
}