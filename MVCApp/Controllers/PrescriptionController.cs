using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCApp.Services.Interfaces;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    // Handles prescription pages for the logged-in doctor.
    [Authorize(Roles = "Doctor")]
    [Route("Doctor")]
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PrescriptionController(
            IPrescriptionService prescriptionService,
            UserManager<ApplicationUser> userManager)
        {
            _prescriptionService = prescriptionService;
            _userManager = userManager;
        }

        // Shows prescriptions created through doctor visit records.
        [HttpGet("Prescriptions")]
        public async Task<IActionResult> Prescriptions()
        {
            ViewData["Title"] = "My Prescriptions";

            var model = await _prescriptionService.GetDoctorPrescriptionsAsync(GetCurrentUserId());
            if (model == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            return View("~/Views/Doctor/Prescriptions.cshtml", model);
        }

        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }
    }
}
