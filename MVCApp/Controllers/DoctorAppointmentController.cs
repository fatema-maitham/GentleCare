using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Doctor;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    // Handles Doctor appointment pages: list, details, status update, patient history, and follow-up requests.
    [Authorize(Roles = "Doctor")]
    [Route("Doctor")]
    public class DoctorAppointmentController : Controller
    {
        private readonly IDoctorAppointmentService _doctorAppointmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorAppointmentController(
            IDoctorAppointmentService doctorAppointmentService,
            UserManager<ApplicationUser> userManager)
        {
            _doctorAppointmentService = doctorAppointmentService;
            _userManager = userManager;
        }

        // Lists only the logged-in doctor's appointments.
        [HttpGet("Appointments")]
        public async Task<IActionResult> Appointments(
            string? searchTerm = null,
            string? status = null,
            DateTime? date = null)
        {
            ViewData["Title"] = "My Appointments";

            var model = await _doctorAppointmentService.GetAppointmentsAsync(
                GetCurrentUserId(),
                searchTerm,
                status,
                date);

            if (model == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            return View("~/Views/Doctor/Appointments.cshtml", model);
        }

        // Shows full appointment details.
        [HttpGet("AppointmentDetails/{id:int}")]
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            ViewData["Title"] = "Appointment Details";

            var model = await _doctorAppointmentService.GetAppointmentDetailsAsync(
                GetCurrentUserId(),
                id);

            if (model == null)
            {
                return NotFound();
            }

            return View("~/Views/Doctor/AppointmentDetails.cshtml", model);
        }

        // Opens update-status page with valid next statuses only.
        [HttpGet("UpdateStatus/{id:int}")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            ViewData["Title"] = "Update Appointment Status";

            var model = await _doctorAppointmentService.GetUpdateStatusModelAsync(
                GetCurrentUserId(),
                id);

            if (model == null)
            {
                return NotFound();
            }

            if (!model.HasAvailableStatuses)
            {
                TempData["Error"] = "This appointment cannot be updated by the doctor at its current status.";
                return RedirectToAction(nameof(AppointmentDetails), new { id });
            }

            return View("~/Views/Doctor/UpdateStatus.cshtml", model);
        }

        // Saves appointment status update.
        [HttpPost("UpdateStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateAppointmentStatusViewModel model)
        {
            ViewData["Title"] = "Update Appointment Status";

            if (!ModelState.IsValid)
            {
                var freshModel = await _doctorAppointmentService.GetUpdateStatusModelAsync(
                    GetCurrentUserId(),
                    model.AppointmentId);

                if (freshModel != null)
                {
                    model.CurrentStatusName = freshModel.CurrentStatusName;
                    model.AvailableStatuses = freshModel.AvailableStatuses;
                }

                return View("~/Views/Doctor/UpdateStatus.cshtml", model);
            }

            var result = await _doctorAppointmentService.UpdateStatusAsync(
                GetCurrentUserId(),
                model);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ?? "Unable to update appointment status.");

                var freshModel = await _doctorAppointmentService.GetUpdateStatusModelAsync(
                    GetCurrentUserId(),
                    model.AppointmentId);

                if (freshModel != null)
                {
                    model.CurrentStatusName = freshModel.CurrentStatusName;
                    model.AvailableStatuses = freshModel.AvailableStatuses;
                }

                return View("~/Views/Doctor/UpdateStatus.cshtml", model);
            }

            TempData["Success"] = "Appointment status updated successfully.";

            return RedirectToAction(nameof(AppointmentDetails), new { id = model.AppointmentId });
        }

        // Shows visit history for a patient linked to the logged-in doctor.
        [HttpGet("PatientHistory/{patientId:int}")]
        public async Task<IActionResult> PatientHistory(int patientId)
        {
            ViewData["Title"] = "Patient History";

            var model = await _doctorAppointmentService.GetPatientHistoryAsync(
                GetCurrentUserId(),
                patientId);

            if (model == null)
            {
                return NotFound();
            }

            return View("~/Views/Doctor/PatientHistory.cshtml", model);
        }

        // Opens the follow-up request form for a completed appointment.
        // Absolute route fixes HTTP 405 for:
        // /Doctor/CreateFollowUpRequest/23
        [HttpGet("/Doctor/CreateFollowUpRequest/{appointmentId:int}")]
        public async Task<IActionResult> CreateFollowUpRequest(int appointmentId)
        {
            ViewData["Title"] = "Create Follow-Up Request";

            var model = await _doctorAppointmentService.GetCreateFollowUpRequestAsync(
                GetCurrentUserId(),
                appointmentId);

            if (model == null)
            {
                TempData["Error"] = "Follow-up request can only be created for completed appointments.";

                return RedirectToAction(
                    nameof(AppointmentDetails),
                    new { id = appointmentId });
            }

            return View("~/Views/Doctor/CreateFollowUpRequest.cshtml", model);
        }

        // Also supports:
        // /Doctor/CreateFollowUpRequest?appointmentId=23
        [HttpGet("/Doctor/CreateFollowUpRequest")]
        public async Task<IActionResult> CreateFollowUpRequestByQuery(int appointmentId)
        {
            ViewData["Title"] = "Create Follow-Up Request";

            if (appointmentId <= 0)
            {
                TempData["Error"] = "Appointment was not selected.";
                return RedirectToAction(nameof(Appointments));
            }

            var model = await _doctorAppointmentService.GetCreateFollowUpRequestAsync(
                GetCurrentUserId(),
                appointmentId);

            if (model == null)
            {
                TempData["Error"] = "Follow-up request can only be created for completed appointments.";

                return RedirectToAction(
                    nameof(AppointmentDetails),
                    new { id = appointmentId });
            }

            return View("~/Views/Doctor/CreateFollowUpRequest.cshtml", model);
        }

        // Saves the follow-up request as a new requested appointment.
        [HttpPost("/Doctor/CreateFollowUpRequest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFollowUpRequest(CreateFollowUpRequestViewModel model)
        {
            ViewData["Title"] = "Create Follow-Up Request";

            if (!ModelState.IsValid)
            {
                await ReloadFollowUpDisplayDataAsync(model);

                return View("~/Views/Doctor/CreateFollowUpRequest.cshtml", model);
            }

            var result = await _doctorAppointmentService.CreateFollowUpRequestAsync(
                GetCurrentUserId(),
                model);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ?? "Unable to create follow-up request.");

                await ReloadFollowUpDisplayDataAsync(model);

                return View("~/Views/Doctor/CreateFollowUpRequest.cshtml", model);
            }

            TempData["Success"] = "Follow-up appointment request created successfully.";

            return RedirectToAction(
                nameof(AppointmentDetails),
                new { id = result.NewAppointmentId });
        }

        // Reloads readonly display fields when validation fails.
        private async Task ReloadFollowUpDisplayDataAsync(CreateFollowUpRequestViewModel model)
        {
            var freshModel = await _doctorAppointmentService.GetCreateFollowUpRequestAsync(
                GetCurrentUserId(),
                model.OriginalAppointmentId);

            if (freshModel == null)
            {
                return;
            }

            model.PatientId = freshModel.PatientId;
            model.PatientFullName = freshModel.PatientFullName;
            model.DoctorId = freshModel.DoctorId;
            model.DoctorFullName = freshModel.DoctorFullName;
            model.OriginalAppointmentDate = freshModel.OriginalAppointmentDate;
        }

        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }
    }
}