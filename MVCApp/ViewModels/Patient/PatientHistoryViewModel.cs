using System;

namespace MVCApp.ViewModels.Patient
{
    public class PatientHistoryViewModel
    {
        public DateTime AppointmentDate { get; set; }
        public string DoctorName { get; set; }
        public string Diagnosis { get; set; }
    }
}