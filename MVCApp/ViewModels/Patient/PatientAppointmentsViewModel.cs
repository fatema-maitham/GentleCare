using System;

namespace MVCApp.ViewModels.Patient
{
    public class PatientAppointmentsViewModel
    {
        public DateTime Date { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; }
    }
}