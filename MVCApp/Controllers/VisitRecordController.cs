using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Doctor;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    // Handles create/edit visit record pages for doctors.
    [Authorize(Roles = "Doctor")]
    [Route("Doctor")]
    public class VisitRecordController : Controller
    {
        private readonly IVisitRecordService _visitRecordService;
        private readonly UserManager<ApplicationUser> _userManager;

        public VisitRecordController(
            IVisitRecordService visitRecordService,
            UserManager<ApplicationUser> userManager)
        {
            _visitRecordService = visitRecordService;
            _userManager = userManager;
        }

        // Opens visit record creation form.
        [HttpGet("CreateVisitRecord/{appointmentId:int}")]
        public async Task<IActionResult> CreateVisitRecord(int appointmentId)
        {
            ViewData["Title"] = "Create Visit Record";

            var model = await _visitRecordService.GetCreateVisitRecordAsync(GetCurrentUserId(), appointmentId);

            if (model == null)
            {
                TempData["Error"] = "Visit record cannot be created for this appointment.";
                return RedirectToAction("AppointmentDetails", "DoctorAppointment", new { id = appointmentId });
            }

            return View("~/Views/Doctor/CreateVisitRecord.cshtml", model);
        }

        // Saves the new visit record and optional prescriptions.
        // This accepts both:
        // /Doctor/CreateVisitRecord
        // /Doctor/CreateVisitRecord/21
        [HttpPost("CreateVisitRecord/{appointmentId:int?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVisitRecord(int? appointmentId, CreateVisitRecordViewModel model)
        {
            ViewData["Title"] = "Create Visit Record";

            if (appointmentId.HasValue && model.AppointmentId == 0)
            {
                model.AppointmentId = appointmentId.Value;
            }

            if (!ModelState.IsValid)
            {
                await RefillCreateVisitRecordMetaAsync(model);
                return View("~/Views/Doctor/CreateVisitRecord.cshtml", model);
            }

            var result = await _visitRecordService.CreateVisitRecordAsync(GetCurrentUserId(), model);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ?? "Unable to create visit record.");

                await RefillCreateVisitRecordMetaAsync(model);
                return View("~/Views/Doctor/CreateVisitRecord.cshtml", model);
            }

            TempData["Success"] = "Visit record created successfully.";

            return RedirectToAction("AppointmentDetails", "DoctorAppointment", new { id = result.AppointmentId });
        }

        // Opens edit visit record form.
        [HttpGet("EditVisitRecord/{appointmentId:int}")]
        public async Task<IActionResult> EditVisitRecord(int appointmentId)
        {
            ViewData["Title"] = "Edit Visit Record";

            var model = await _visitRecordService.GetEditVisitRecordAsync(GetCurrentUserId(), appointmentId);

            if (model == null)
            {
                TempData["Error"] = "No visit record exists for this appointment.";
                return RedirectToAction("AppointmentDetails", "DoctorAppointment", new { id = appointmentId });
            }

            return View("~/Views/Doctor/EditVisitRecord.cshtml", model);
        }

        // Saves edited visit record and prescriptions.
        // This accepts both:
        // /Doctor/EditVisitRecord
        // /Doctor/EditVisitRecord/21
        [HttpPost("EditVisitRecord/{appointmentId:int?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVisitRecord(int? appointmentId, EditVisitRecordViewModel model)
        {
            ViewData["Title"] = "Edit Visit Record";

            if (appointmentId.HasValue && model.AppointmentId == 0)
            {
                model.AppointmentId = appointmentId.Value;
            }

            if (!ModelState.IsValid)
            {
                await RefillEditVisitRecordMetaAsync(model);
                return View("~/Views/Doctor/EditVisitRecord.cshtml", model);
            }

            var result = await _visitRecordService.EditVisitRecordAsync(GetCurrentUserId(), model);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ?? "Unable to update visit record.");

                await RefillEditVisitRecordMetaAsync(model);
                return View("~/Views/Doctor/EditVisitRecord.cshtml", model);
            }

            TempData["Success"] = "Visit record updated successfully.";

            return RedirectToAction("AppointmentDetails", "DoctorAppointment", new { id = result.AppointmentId });
        }

        private async Task RefillCreateVisitRecordMetaAsync(CreateVisitRecordViewModel model)
        {
            var freshModel = await _visitRecordService.GetCreateVisitRecordAsync(
                GetCurrentUserId(),
                model.AppointmentId);

            if (freshModel == null)
            {
                return;
            }

            model.PatientId = freshModel.PatientId;
            model.PatientFullName = freshModel.PatientFullName;
            model.AppointmentDate = freshModel.AppointmentDate;
            model.StartTime = freshModel.StartTime;
            model.EndTime = freshModel.EndTime;

            if (model.Prescriptions == null || model.Prescriptions.Count == 0)
            {
                model.Prescriptions = freshModel.Prescriptions;
            }
        }

        private async Task RefillEditVisitRecordMetaAsync(EditVisitRecordViewModel model)
        {
            var freshModel = await _visitRecordService.GetEditVisitRecordAsync(
                GetCurrentUserId(),
                model.AppointmentId);

            if (freshModel == null)
            {
                return;
            }

            model.PatientId = freshModel.PatientId;
            model.PatientFullName = freshModel.PatientFullName;
            model.AppointmentDate = freshModel.AppointmentDate;
            model.StartTime = freshModel.StartTime;
            model.EndTime = freshModel.EndTime;

            if (model.Prescriptions == null || model.Prescriptions.Count == 0)
            {
                model.Prescriptions = freshModel.Prescriptions;
            }
        }

        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }
    }
}