using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCApp.Services.Interfaces;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    // Handles general Doctor pages only: dashboard, profile, schedule, notifications.
    // Appointment and visit-record logic is moved to separate controllers and services.
    [Authorize(Roles = "Doctor")]
    [Route("Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDoctorDashboardService _doctorDashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(
            IDoctorDashboardService doctorDashboardService,
            UserManager<ApplicationUser> userManager)
        {
            _doctorDashboardService = doctorDashboardService;
            _userManager = userManager;
        }

        // Doctor dashboard with calendar, statistics, and selected-day appointments.
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

        // Displays the logged-in doctor's profile.
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

        // Displays the logged-in doctor's weekly schedule and leaves.
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

        // Shows all notifications for the logged-in doctor.
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

        // Marks one notification as read.
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

        // Marks all notifications as read.
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
