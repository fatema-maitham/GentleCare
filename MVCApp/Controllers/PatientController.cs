using Microsoft.AspNetCore.Mvc;
using MVCApp.ViewModels.Patient;

namespace MVCApp.Controllers
{
    public class PatientController : Controller
    {
        // Patient Profile
        public IActionResult Profile()
        {
            var model = new PatientProfileViewModel
            {
                FullName = "Fatema Maitham",
                CPRNumber = "12345678",
                Email = "fatema@email.com"
            };

            return View(model);
        }

        // Patient History
        public IActionResult History()
        {
            var history = new List<PatientHistoryViewModel>
            {
                new PatientHistoryViewModel
                {
                    AppointmentDate = DateTime.Now.AddDays(-10),
                    DoctorName = "Dr. Ahmed",
                    Diagnosis = "Flu"
                },
                new PatientHistoryViewModel
                {
                    AppointmentDate = DateTime.Now.AddDays(-20),
                    DoctorName = "Dr. Sara",
                    Diagnosis = "Headache"
                }
            };

            return View(history);
        }

        // Patient Appointments
        public IActionResult Appointments()
        {
            var appointments = new List<PatientAppointmentsViewModel>
            {
                new PatientAppointmentsViewModel
                {
                    Date = DateTime.Now.AddDays(2),
                    DoctorName = "Dr. Ahmed",
                    Status = "Confirmed"
                },
                new PatientAppointmentsViewModel
                {
                    Date = DateTime.Now.AddDays(5),
                    DoctorName = "Dr. Sara",
                    Status = "Requested"
                }
            };

            return View(appointments);
        }
    }
}