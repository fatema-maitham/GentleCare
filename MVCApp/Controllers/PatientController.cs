using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Patient;

namespace MVCApp.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var model = await _patientService.GetDashboardAsync(User);

            if (model is null)
            {
                return NotFound("Patient profile not found.");
            }

            return View((object)model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var model = await _patientService.GetProfileAsync(User);

            if (model is null)
            {
                return NotFound("Patient profile not found.");
            }

            return View((object)model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var model = await _patientService.GetEditProfileAsync(User);

            if (model is null)
            {
                return NotFound("Patient profile not found.");
            }

            return View((object)model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(PatientEditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View((object)model);
            }

            var result = await _patientService.UpdateProfileAsync(User, model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View((object)model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            var model = await _patientService.GetAppointmentsAsync(User);
            return View((object)model);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var model = await _patientService.GetHistoryAsync(User);
            return View((object)model);
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment(
            int? specializationId = null,
            int? doctorId = null,
            DateTime? appointmentDate = null)
        {
            var model = await _patientService.GetBookAppointmentModelAsync(
                User,
                specializationId,
                doctorId,
                appointmentDate);

            if (model is null)
            {
                return NotFound("Patient profile not found.");
            }

            return View((object)model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(PatientBookAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var refreshedModel = await _patientService.GetBookAppointmentModelAsync(
                    User,
                    model.SpecializationId,
                    model.DoctorId,
                    model.AppointmentDate);

                if (refreshedModel is null)
                {
                    return NotFound("Patient profile not found.");
                }

                refreshedModel.StartTime = model.StartTime;
                refreshedModel.Notes = model.Notes;

                return View((object)refreshedModel);
            }

            var result = await _patientService.BookAppointmentAsync(User, model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);

                var refreshedModel = await _patientService.GetBookAppointmentModelAsync(
                    User,
                    model.SpecializationId,
                    model.DoctorId,
                    model.AppointmentDate);

                if (refreshedModel is null)
                {
                    return NotFound("Patient profile not found.");
                }

                refreshedModel.StartTime = model.StartTime;
                refreshedModel.Notes = model.Notes;

                return View((object)refreshedModel);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var result = await _patientService.CancelAppointmentAsync(User, appointmentId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Appointments));
        }
    }
}