using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.Doctor;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    [Authorize(Roles = "Doctor")]
    [Route("Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDoctorDashboardService _doctorDashboardService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public DoctorController(
            IDoctorDashboardService doctorDashboardService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _doctorDashboardService = doctorDashboardService;
            _userManager = userManager;
            _environment = environment;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard(DateTime? selectedDate = null)
        {
            ViewData["Title"] = "Doctor Dashboard";

            var model = await _doctorDashboardService.GetDashboardAsync(GetCurrentUserId(), selectedDate);

            if (model == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(model);
        }

        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            ViewData["Title"] = "My Profile";

            var model = await _doctorDashboardService.GetProfileAsync(GetCurrentUserId());

            if (model == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(model);
        }

        [HttpGet("EditProfile")]
        public async Task<IActionResult> EditProfile()
        {
            ViewData["Title"] = "Edit Profile";

            var model = await _doctorDashboardService.GetEditProfileAsync(GetCurrentUserId());

            if (model == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(model);
        }

        [HttpPost("EditProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditDoctorProfileViewModel model)
        {
            ViewData["Title"] = "Edit Profile";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updated = await _doctorDashboardService.UpdateProfileAsync(
                GetCurrentUserId(),
                model,
                _environment.WebRootPath);

            if (!updated)
            {
                TempData["Error"] = "Profile could not be updated. Upload JPG, PNG, or WEBP only.";
                return View(model);
            }

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet("Schedule")]
        public async Task<IActionResult> Schedule()
        {
            ViewData["Title"] = "My Schedule";

            var model = await _doctorDashboardService.GetScheduleAsync(GetCurrentUserId());

            if (model == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(model);
        }

        [HttpGet("Notifications")]
        public async Task<IActionResult> Notifications()
        {
            ViewData["Title"] = "My Notifications";

            var model = await _doctorDashboardService.GetNotificationsAsync(GetCurrentUserId());

            if (model == null)
            {
                TempData["Error"] = "Doctor profile was not found for the current user.";
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(model);
        }

        [HttpPost("MarkNotificationAsRead")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var updated = await _doctorDashboardService.MarkNotificationAsReadAsync(GetCurrentUserId(), id);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = "Notification marked as read.";
            return RedirectToAction(nameof(Notifications));
        }

        [HttpPost("MarkAllNotificationsAsRead")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            await _doctorDashboardService.MarkAllNotificationsAsReadAsync(GetCurrentUserId());

            TempData["Success"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Notifications));
        }

        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }
    }
}