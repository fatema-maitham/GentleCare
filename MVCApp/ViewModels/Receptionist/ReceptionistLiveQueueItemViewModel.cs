using System;

namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistLiveQueueItemViewModel
    {
        public int AppointmentId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string CPRNumber { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}