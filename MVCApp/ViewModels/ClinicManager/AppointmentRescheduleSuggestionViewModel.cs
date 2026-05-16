namespace MVCApp.ViewModels.ClinicManager
{
    // Suggested available replacement slot for an impacted appointment.
    public class AppointmentRescheduleSuggestionViewModel
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public DateTime NewDate { get; set; }

        public TimeOnly NewStartTime { get; set; }

        public TimeOnly NewEndTime { get; set; }

        public string SpecializationMatchText { get; set; } = string.Empty;

        public string DateText => NewDate.ToString("dd MMM yyyy");

        public string TimeText => $"{NewStartTime:HH\\:mm} - {NewEndTime:HH\\:mm}";
    }
}