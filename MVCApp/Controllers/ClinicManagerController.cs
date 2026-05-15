using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCApp.Services.Interfaces;
using MVCApp.ViewModels.ClinicManager;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    // Only users in the ClinicManager role can access this controller.
    // This controller stays clean because the business logic is inside ClinicManagerService.
    [Authorize(Roles = "ClinicManager")]
    public class ClinicManagerController : Controller
    {
        private readonly IClinicManagerService _clinicManagerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClinicManagerController(
            IClinicManagerService clinicManagerService,
            UserManager<ApplicationUser> userManager)
        {
            _clinicManagerService = clinicManagerService;
            _userManager = userManager;
        }

        // =========================
        // Dashboard
        // =========================

        // GET: /ClinicManager/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Clinic Manager Dashboard";

            var model = await _clinicManagerService.GetDashboardAsync();

            return View(model);
        }

        // =========================
        // Doctor Management
        // =========================

        // GET: /ClinicManager/Doctors
        [HttpGet]
        public async Task<IActionResult> Doctors(string? searchTerm, bool? isActive)
        {
            ViewData["Title"] = "Doctors";

            var model = await _clinicManagerService.GetDoctorsAsync(searchTerm, isActive);

            return View(model);
        }

        // GET: /ClinicManager/DoctorDetails/5
        [HttpGet]
        public async Task<IActionResult> DoctorDetails(int id)
        {
            ViewData["Title"] = "Doctor Details";

            var model = await _clinicManagerService.GetDoctorDetailsAsync(id);

            if (model == null)
            {
                TempData["Error"] = "Doctor was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // GET: /ClinicManager/CreateDoctor
        [HttpGet]
        public async Task<IActionResult> CreateDoctor()
        {
            ViewData["Title"] = "Create Doctor";

            var model = await _clinicManagerService.GetCreateDoctorViewModelAsync();

            return View(model);
        }

        // POST: /ClinicManager/CreateDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(DoctorCreateViewModel model)
        {
            ViewData["Title"] = "Create Doctor";

            if (!ModelState.IsValid)
            {
                // Reload dropdown/checkbox data if validation fails.
                var reloadModel = await _clinicManagerService.GetCreateDoctorViewModelAsync();
                model.SpecializationOptions = reloadModel.SpecializationOptions;

                return View(model);
            }

            var result = await _clinicManagerService.CreateDoctorAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);

                // Reload dropdown/checkbox data if service validation fails.
                var reloadModel = await _clinicManagerService.GetCreateDoctorViewModelAsync();
                model.SpecializationOptions = reloadModel.SpecializationOptions;

                return View(model);
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(DoctorDetails), new { id = result.DoctorId });
        }

        // GET: /ClinicManager/EditDoctor/5
        [HttpGet]
        public async Task<IActionResult> EditDoctor(int id)
        {
            ViewData["Title"] = "Edit Doctor";

            var model = await _clinicManagerService.GetEditDoctorViewModelAsync(id);

            if (model == null)
            {
                TempData["Error"] = "Doctor was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // POST: /ClinicManager/EditDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(DoctorEditViewModel model)
        {
            ViewData["Title"] = "Edit Doctor";

            if (!ModelState.IsValid)
            {
                // Reload dropdown/checkbox data if validation fails.
                var reloadModel = await _clinicManagerService.GetEditDoctorViewModelAsync(model.DoctorId);

                if (reloadModel != null)
                {
                    model.SpecializationOptions = reloadModel.SpecializationOptions;
                }

                return View(model);
            }

            var result = await _clinicManagerService.EditDoctorAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);

                // Reload dropdown/checkbox data if service validation fails.
                var reloadModel = await _clinicManagerService.GetEditDoctorViewModelAsync(model.DoctorId);

                if (reloadModel != null)
                {
                    model.SpecializationOptions = reloadModel.SpecializationOptions;
                }

                return View(model);
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(DoctorDetails), new { id = model.DoctorId });
        }

        // =========================
        // Doctor Schedule Management
        // =========================

        // GET: /ClinicManager/ManageDoctorSchedule?doctorId=5
        [HttpGet]
        public async Task<IActionResult> ManageDoctorSchedule(int doctorId)
        {
            ViewData["Title"] = "Doctor Schedule";

            var model = await _clinicManagerService.GetDoctorScheduleAsync(doctorId);

            if (model == null)
            {
                TempData["Error"] = "Doctor was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // GET: /ClinicManager/CreateDoctorSchedule?doctorId=5
        [HttpGet]
        public async Task<IActionResult> CreateDoctorSchedule(int doctorId)
        {
            ViewData["Title"] = "Create Doctor Schedule";

            var model = await _clinicManagerService.GetCreateScheduleViewModelAsync(doctorId);

            if (model == null)
            {
                TempData["Error"] = "Doctor was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // POST: /ClinicManager/CreateDoctorSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctorSchedule(DoctorScheduleFormViewModel model)
        {
            ViewData["Title"] = "Create Doctor Schedule";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _clinicManagerService.CreateScheduleAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;

            // After schedule changes, manager should review affected appointments.
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = model.DoctorId });
        }

        // GET: /ClinicManager/EditDoctorSchedule/5
        [HttpGet]
        public async Task<IActionResult> EditDoctorSchedule(int id)
        {
            ViewData["Title"] = "Edit Doctor Schedule";

            var model = await _clinicManagerService.GetEditScheduleViewModelAsync(id);

            if (model == null)
            {
                TempData["Error"] = "Schedule was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // POST: /ClinicManager/EditDoctorSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctorSchedule(DoctorScheduleFormViewModel model)
        {
            ViewData["Title"] = "Edit Doctor Schedule";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _clinicManagerService.EditScheduleAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;

            // After schedule changes, manager should review affected appointments.
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = result.DoctorId });
        }

        // POST: /ClinicManager/DeleteDoctorSchedule/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctorSchedule(int id)
        {
            var result = await _clinicManagerService.DeleteScheduleAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Doctors));
            }

            TempData["Success"] = result.Message;

            // After deleting a schedule, manager should review affected appointments.
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = result.DoctorId });
        }

        // =========================
        // Doctor Leave Management
        // =========================

        // GET: /ClinicManager/ManageDoctorLeaves?doctorId=5
        [HttpGet]
        public async Task<IActionResult> ManageDoctorLeaves(int doctorId)
        {
            ViewData["Title"] = "Doctor Leaves";

            var model = await _clinicManagerService.GetDoctorLeavesAsync(doctorId);

            if (model == null)
            {
                TempData["Error"] = "Doctor was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // GET: /ClinicManager/CreateDoctorLeave?doctorId=5
        [HttpGet]
        public async Task<IActionResult> CreateDoctorLeave(int doctorId)
        {
            ViewData["Title"] = "Create Doctor Leave";

            var model = await _clinicManagerService.GetCreateLeaveViewModelAsync(doctorId);

            if (model == null)
            {
                TempData["Error"] = "Doctor was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // POST: /ClinicManager/CreateDoctorLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctorLeave(DoctorLeaveFormViewModel model)
        {
            ViewData["Title"] = "Create Doctor Leave";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _clinicManagerService.CreateLeaveAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;

            // After adding leave, manager should review affected appointments.
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = model.DoctorId });
        }

        // GET: /ClinicManager/EditDoctorLeave/5
        [HttpGet]
        public async Task<IActionResult> EditDoctorLeave(int id)
        {
            ViewData["Title"] = "Edit Doctor Leave";

            var model = await _clinicManagerService.GetEditLeaveViewModelAsync(id);

            if (model == null)
            {
                TempData["Error"] = "Leave period was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // POST: /ClinicManager/EditDoctorLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctorLeave(DoctorLeaveFormViewModel model)
        {
            ViewData["Title"] = "Edit Doctor Leave";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _clinicManagerService.EditLeaveAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;

            // After changing leave, manager should review affected appointments.
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = result.DoctorId });
        }

        // POST: /ClinicManager/DeleteDoctorLeave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctorLeave(int id)
        {
            var result = await _clinicManagerService.DeleteLeaveAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Doctors));
            }

            TempData["Success"] = result.Message;

            // After deleting leave, manager should review affected appointments.
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = result.DoctorId });
        }

        // =========================
        // Appointment Impact
        // =========================

        // GET: /ClinicManager/AppointmentImpact?doctorId=5
        [HttpGet]
        public async Task<IActionResult> AppointmentImpact(
            int doctorId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ViewData["Title"] = "Appointment Impact";

            var model = await _clinicManagerService.GetAppointmentImpactAsync(
                doctorId,
                fromDate,
                toDate);

            if (model == null)
            {
                TempData["Error"] = "Doctor was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // POST: /ClinicManager/CancelImpactedAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelImpactedAppointment(int appointmentId, string? reason)
        {
            var result = await _clinicManagerService.CancelImpactedAppointmentAsync(
                appointmentId,
                reason);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                if (result.DoctorId.HasValue)
                {
                    return RedirectToAction(nameof(AppointmentImpact), new { doctorId = result.DoctorId.Value });
                }

                return RedirectToAction(nameof(Doctors));
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = result.DoctorId });
        }

        // =========================
        // Clinic Appointment Management
        // =========================

        // GET: /ClinicManager/Appointments
        [HttpGet]
        public async Task<IActionResult> Appointments(
            int? doctorId,
            string? status,
            DateTime? date,
            string? searchTerm)
        {
            ViewData["Title"] = "Clinic Appointments";

            var model = await _clinicManagerService.GetAppointmentsAsync(
                doctorId,
                status,
                date,
                searchTerm);

            return View(model);
        }

        // GET: /ClinicManager/AppointmentDetails/5
        [HttpGet]
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            ViewData["Title"] = "Appointment Details";

            var model = await _clinicManagerService.GetAppointmentDetailsAsync(id);

            if (model == null)
            {
                TempData["Error"] = "Appointment was not found.";
                return RedirectToAction(nameof(Appointments));
            }

            return View(model);
        }

        // GET: /ClinicManager/UpdateAppointmentStatus/5
        [HttpGet]
        public async Task<IActionResult> UpdateAppointmentStatus(int id)
        {
            ViewData["Title"] = "Update Appointment Status";

            var model = await _clinicManagerService.GetAppointmentStatusViewModelAsync(id);

            if (model == null)
            {
                TempData["Error"] = "Appointment was not found.";
                return RedirectToAction(nameof(Appointments));
            }

            if (!model.HasStatusOptions)
            {
                TempData["Error"] = "This appointment status cannot be updated.";
                return RedirectToAction(nameof(AppointmentDetails), new { id });
            }

            return View(model);
        }

        // POST: /ClinicManager/UpdateAppointmentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(ClinicManagerAppointmentStatusViewModel model)
        {
            ViewData["Title"] = "Update Appointment Status";

            if (!ModelState.IsValid)
            {
                // Reload valid status options if validation fails.
                var reloadModel = await _clinicManagerService.GetAppointmentStatusViewModelAsync(model.AppointmentId);

                if (reloadModel != null)
                {
                    model.StatusOptions = reloadModel.StatusOptions;
                }

                return View(model);
            }

            var result = await _clinicManagerService.UpdateAppointmentStatusAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);

                // Reload valid status options if service validation fails.
                var reloadModel = await _clinicManagerService.GetAppointmentStatusViewModelAsync(model.AppointmentId);

                if (reloadModel != null)
                {
                    model.StatusOptions = reloadModel.StatusOptions;
                }

                return View(model);
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(AppointmentDetails), new { id = model.AppointmentId });
        }

        // =========================
        // Doctor Specializations
        // =========================

        // GET: /ClinicManager/ManageDoctorSpecializations?doctorId=5
        [HttpGet]
        public async Task<IActionResult> ManageDoctorSpecializations(int doctorId)
        {
            ViewData["Title"] = "Manage Doctor Specializations";

            var model = await _clinicManagerService.GetDoctorSpecializationsAsync(doctorId);

            if (model == null)
            {
                TempData["Error"] = "Doctor was not found.";
                return RedirectToAction(nameof(Doctors));
            }

            return View(model);
        }

        // POST: /ClinicManager/ManageDoctorSpecializations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageDoctorSpecializations(ManageDoctorSpecializationsViewModel model)
        {
            ViewData["Title"] = "Manage Doctor Specializations";

            var result = await _clinicManagerService.UpdateDoctorSpecializationsAsync(model);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Doctors));
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(nameof(DoctorDetails), new { id = model.DoctorId });
        }

        // =========================
        // Reports
        // =========================

        // GET: /ClinicManager/Reports
        [HttpGet]
        public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate)
        {
            ViewData["Title"] = "Clinic Reports";

            var model = await _clinicManagerService.GetReportsAsync(fromDate, toDate);

            return View(model);
        }

        // =========================
        // Notifications
        // =========================

        // GET: /ClinicManager/Notifications
        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            ViewData["Title"] = "Notifications";

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = await _clinicManagerService.GetNotificationsAsync(user.Id);

            return View(model);
        }

        // POST: /ClinicManager/MarkNotificationAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var success = await _clinicManagerService.MarkNotificationAsReadAsync(id, user.Id);

            if (!success)
            {
                TempData["Error"] = "Notification was not found.";
            }

            return RedirectToAction(nameof(Notifications));
        }

        // POST: /ClinicManager/MarkAllNotificationsAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await _clinicManagerService.MarkAllNotificationsAsReadAsync(user.Id);

            TempData["Success"] = "All notifications marked as read.";

            return RedirectToAction(nameof(Notifications));
        }
    }
}