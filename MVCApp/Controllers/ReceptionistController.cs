using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Receptionist;

namespace MVCApp.Controllers
{
    [Authorize(Roles = "Receptionist")]
    public class ReceptionistController : Controller
    {
        private readonly IReceptionistService _receptionistService;

        public ReceptionistController(IReceptionistService receptionistService)
        {
            _receptionistService = receptionistService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _receptionistService.GetDashboardAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var model = await _receptionistService.GetProfileAsync(User);

            if (model == null)
            {
                return NotFound("Receptionist profile not found.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var model = await _receptionistService.GetEditProfileAsync(User);

            if (model == null)
            {
                return NotFound("Receptionist profile not found.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ReceptionistEditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var currentModel = await _receptionistService.GetEditProfileAsync(User);

                if (currentModel != null)
                {
                    model.CurrentProfilePicture = currentModel.CurrentProfilePicture;
                }

                return View(model);
            }

            var result = await _receptionistService.UpdateProfileAsync(User, model);

            if (!result.Success)
            {
                var currentModel = await _receptionistService.GetEditProfileAsync(User);

                if (currentModel != null)
                {
                    model.CurrentProfilePicture = currentModel.CurrentProfilePicture;
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> Appointments(string? searchText, DateTime? selectedDate, string? selectedStatus)
        {
            var model = await _receptionistService.GetAppointmentsAsync(searchText, selectedDate, selectedStatus);
            ViewBag.SelectedStatus = selectedStatus;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment(
            int? patientId,
            int? specializationId,
            int? doctorId,
            DateTime? appointmentDate)
        {
            var model = await _receptionistService.GetBookAppointmentModelAsync(
                patientId,
                specializationId,
                doctorId,
                appointmentDate);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(ReceptionistBookAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var refreshedModel = await _receptionistService.GetBookAppointmentModelAsync(
                    model.PatientId,
                    model.SpecializationId,
                    model.DoctorId,
                    model.AppointmentDate);

                refreshedModel.SelectedStartTime = model.SelectedStartTime;
                refreshedModel.Notes = model.Notes;

                return View(refreshedModel);
            }

            var result = await _receptionistService.BookAppointmentAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);

                var refreshedModel = await _receptionistService.GetBookAppointmentModelAsync(
                    model.PatientId,
                    model.SpecializationId,
                    model.DoctorId,
                    model.AppointmentDate);

                refreshedModel.SelectedStartTime = model.SelectedStartTime;
                refreshedModel.Notes = model.Notes;

                return View(refreshedModel);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Appointments));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var model = await _receptionistService.GetUpdateStatusModelAsync(id);

            if (model == null)
            {
                return NotFound("Appointment not found.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(ReceptionistUpdateAppointmentStatusViewModel model)
        {
            var result = await _receptionistService.UpdateStatusAsync(model);

            if (!result.Success)
            {
                if (result.Model == null)
                {
                    return NotFound("Appointment not found.");
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View(result.Model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Appointments));
        }

        [HttpGet]
        public async Task<IActionResult> PatientSearch(string? searchText)
        {
            var model = await _receptionistService.SearchPatientsAsync(searchText);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LiveQueue()
        {
            var model = await _receptionistService.GetLiveQueueAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQueueStatus(int appointmentId, string newStatus)
        {
            var result = await _receptionistService.UpdateQueueStatusAsync(appointmentId, newStatus);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(LiveQueue));
        }
    }
}