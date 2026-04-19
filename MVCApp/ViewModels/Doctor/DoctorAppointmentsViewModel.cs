using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.Doctor
{
    public class DoctorAppointmentsViewModel
    {
        public string DoctorFullName { get; set; } = string.Empty;

        public string? SelectedStatus { get; set; }
        public DateTime? SelectedDate { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();
        public List<DoctorAppointmentListItemViewModel> Appointments { get; set; } = new();
    }

    public class DoctorAppointmentListItemViewModel
    {
        public int AppointmentId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string PatientReferenceNumber { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}