using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCApp.ViewModels.Doctor;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var vm = new DoctorProfileViewModel
            {
                Id = doctor.Id,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio,
                Specializations = doctor.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name)
                    .OrderBy(x => x)
                    .ToList(),
                Schedules = doctor.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .Select(s => new DoctorScheduleItemViewModel
                    {
                        Id = s.Id,
                        DayOfWeek = s.DayOfWeek.ToString(),
                        StartTime = s.StartTime.ToString("HH\\:mm"),
                        EndTime = s.EndTime.ToString("HH\\:mm"),
                        SlotDurationMinutes = s.SlotDurationMinutes
                    }).ToList(),
                Leaves = doctor.Leaves
                    .OrderBy(l => l.StartDate)
                    .Select(l => new DoctorLeaveItemViewModel
                    {
                        Id = l.Id,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        Reason = l.Reason
                    }).ToList()
            };

            return View(vm);
        }
    }
}
