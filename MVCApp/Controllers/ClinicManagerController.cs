using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCApp.ViewModels.ClinicManager;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    [Authorize(Roles = "ClinicManager")]
    public class ClinicManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ClinicManagerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Clinic Manager Dashboard";

            var today = DateTime.Today;

            var model = new ClinicManagerDashboardViewModel
            {
                TotalDoctors = await _context.Doctors.CountAsync(),
                TotalPatients = await _context.Patients.CountAsync(),
                AppointmentsToday = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today),
                RequestedAppointments = await _context.Appointments
                    .Include(a => a.Status)
                    .CountAsync(a => a.Status.Name == "Requested"),
                DoctorsOnLeaveToday = await _context.DoctorLeaves
                    .CountAsync(l => l.StartDate.Date <= today && l.EndDate.Date >= today)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Doctors(string? search = null)
        {
            ViewData["Title"] = "Manage Doctors";

            var doctors = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Appointments)
                .OrderBy(d => d.User.FullName)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();

                doctors = doctors
                    .Where(d =>
                        d.User.FullName.ToLower().Contains(term) ||
                        (d.User.Email ?? string.Empty).ToLower().Contains(term) ||
                        d.LicenseNumber.ToLower().Contains(term))
                    .ToList();
            }

            var model = new ClinicManagerDoctorsViewModel
            {
                Search = search,
                Doctors = doctors.Select(d => new DoctorListItemViewModel
                {
                    DoctorId = d.Id,
                    UserId = d.UserId,
                    FullName = d.User.FullName,
                    Email = d.User.Email ?? string.Empty,
                    LicenseNumber = d.LicenseNumber,
                    IsActive = d.User.IsActive,
                    Bio = d.Bio,
                    Specializations = d.DoctorSpecializations
                        .Select(ds => ds.Specialization.Name)
                        .OrderBy(x => x)
                        .ToList(),
                    UpcomingAppointmentsCount = d.Appointments.Count(a => a.AppointmentDate.Date >= DateTime.Today)
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DoctorDetails(int id)
        {
            ViewData["Title"] = "Doctor Details";

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Status)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            var model = new DoctorDetailsViewModel
            {
                DoctorId = doctor.Id,
                UserId = doctor.UserId,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio,
                IsActive = doctor.User.IsActive,
                Specializations = doctor.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name)
                    .OrderBy(x => x)
                    .ToList(),
                Schedules = doctor.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new DoctorScheduleItemViewModel
                    {
                        DoctorScheduleId = s.Id,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        SlotDurationMinutes = s.SlotDurationMinutes
                    })
                    .ToList(),
                Leaves = doctor.Leaves
                    .OrderByDescending(l => l.StartDate)
                    .Select(l => new DoctorLeaveItemViewModel
                    {
                        DoctorLeaveId = l.Id,
                        LeaveStartDate = l.StartDate,
                        LeaveEndDate = l.EndDate,
                        LeaveReason = l.Reason
                    })
                    .ToList(),
                UpcomingAppointmentsCount = doctor.Appointments.Count(a =>
                    a.AppointmentDate.Date >= DateTime.Today &&
                    a.Status.Name != "Cancelled" &&
                    a.Status.Name != "Completed" &&
                    a.Status.Name != "Missed")
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult CreateDoctor()
        {
            ViewData["Title"] = "Create Doctor";

            return View(new UpsertDoctorViewModel
            {
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(UpsertDoctorViewModel model)
        {
            ViewData["Title"] = "Create Doctor";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email already exists.");
            }

            if (await _context.Doctors.AnyAsync(d => d.LicenseNumber == model.LicenseNumber))
            {
                ModelState.AddModelError(nameof(model.LicenseNumber), "License number already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync("Doctor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Doctor"));
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim(),
                UserName = model.Email.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var createUserResult = await _userManager.CreateAsync(user, model.Password!);

            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, "Doctor");

            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            var doctor = new Doctor
            {
                UserId = user.Id,
                LicenseNumber = model.LicenseNumber.Trim(),
                Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            try
            {
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, "Doctor profile could not be created.");
                return View(model);
            }

            TempData["Success"] = "Doctor created successfully.";
            return RedirectToAction(nameof(DoctorDetails), new { id = doctor.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditDoctor(int id)
        {
            ViewData["Title"] = "Edit Doctor";

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(new UpsertDoctorViewModel
            {
                DoctorId = doctor.Id,
                UserId = doctor.UserId,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email ?? string.Empty,
                LicenseNumber = doctor.LicenseNumber,
                Bio = doctor.Bio,
                IsActive = doctor.User.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(UpsertDoctorViewModel model)
        {
            ViewData["Title"] = "Edit Doctor";

            if (!model.DoctorId.HasValue)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == model.DoctorId.Value);

            if (doctor == null)
            {
                return NotFound();
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != doctor.UserId))
            {
                ModelState.AddModelError(nameof(model.Email), "Email already exists.");
            }

            if (await _context.Doctors.AnyAsync(d => d.LicenseNumber == model.LicenseNumber && d.Id != doctor.Id))
            {
                ModelState.AddModelError(nameof(model.LicenseNumber), "License number already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            doctor.User.FullName = model.FullName.Trim();
            doctor.User.Email = model.Email.Trim();
            doctor.User.UserName = model.Email.Trim();
            doctor.User.IsActive = model.IsActive;

            doctor.LicenseNumber = model.LicenseNumber.Trim();
            doctor.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Doctor updated successfully.";
            return RedirectToAction(nameof(DoctorDetails), new { id = doctor.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ManageSpecializations(int doctorId)
        {
            ViewData["Title"] = "Manage Specializations";

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            var selectedIds = doctor.DoctorSpecializations
                .Select(ds => ds.SpecializationId)
                .ToHashSet();

            var specializations = await _context.Specializations
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();

            var model = new ManageDoctorSpecializationsViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                SelectedSpecializationIds = selectedIds.ToList(),
                Specializations = specializations.Select(s => new SpecializationSelectionViewModel
                {
                    SpecializationId = s.Id,
                    SpecializationName = s.Name,
                    IsSelected = selectedIds.Contains(s.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageSpecializations(ManageDoctorSpecializationsViewModel model)
        {
            ViewData["Title"] = "Manage Specializations";

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.Id == model.DoctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            var selectedIds = (model.SelectedSpecializationIds ?? new List<int>())
                .Distinct()
                .ToList();

            _context.DoctorSpecializations.RemoveRange(doctor.DoctorSpecializations);

            foreach (var specializationId in selectedIds)
            {
                _context.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorId = doctor.Id,
                    SpecializationId = specializationId
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Doctor specializations updated successfully.";
            return RedirectToAction(nameof(DoctorDetails), new { id = doctor.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ManageSchedule(int doctorId)
        {
            ViewData["Title"] = "Manage Schedule";

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            var model = new ManageDoctorScheduleViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                Schedules = doctor.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new DoctorScheduleItemViewModel
                    {
                        DoctorScheduleId = s.Id,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        SlotDurationMinutes = s.SlotDurationMinutes
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddSchedule(int doctorId)
        {
            ViewData["Title"] = "Add Schedule";

            var exists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);
            if (!exists)
            {
                return NotFound();
            }

            return View(new UpsertDoctorScheduleViewModel
            {
                DoctorId = doctorId,
                SlotDurationMinutes = 30
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSchedule(UpsertDoctorScheduleViewModel model)
        {
            ViewData["Title"] = "Add Schedule";

            var doctor = await _context.Doctors
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.Id == model.DoctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            ValidateScheduleModel(model);

            if (HasScheduleOverlap(
                doctor.Schedules,
                model.DayOfWeek,
                model.StartTime,
                model.EndTime,
                null))
            {
                ModelState.AddModelError(string.Empty, "This schedule overlaps with an existing schedule for the same day.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = model.DoctorId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                SlotDurationMinutes = model.SlotDurationMinutes
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule added successfully.";
            TempData["Info"] = "Review appointment impact after changing doctor availability.";
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = model.DoctorId });
        }

        [HttpGet]
        public async Task<IActionResult> EditSchedule(int id)
        {
            ViewData["Title"] = "Edit Schedule";

            var schedule = await _context.DoctorSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(new UpsertDoctorScheduleViewModel
            {
                DoctorScheduleId = schedule.Id,
                DoctorId = schedule.DoctorId,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                SlotDurationMinutes = schedule.SlotDurationMinutes
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSchedule(UpsertDoctorScheduleViewModel model)
        {
            ViewData["Title"] = "Edit Schedule";

            if (!model.DoctorScheduleId.HasValue)
            {
                return NotFound();
            }

            var schedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.Id == model.DoctorScheduleId.Value);

            if (schedule == null)
            {
                return NotFound();
            }

            var existingSchedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == schedule.DoctorId)
                .ToListAsync();

            ValidateScheduleModel(model);

            if (HasScheduleOverlap(
                existingSchedules,
                model.DayOfWeek,
                model.StartTime,
                model.EndTime,
                schedule.Id))
            {
                ModelState.AddModelError(string.Empty, "This schedule overlaps with an existing schedule for the same day.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            schedule.DayOfWeek = model.DayOfWeek;
            schedule.StartTime = model.StartTime;
            schedule.EndTime = model.EndTime;
            schedule.SlotDurationMinutes = model.SlotDurationMinutes;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule updated successfully.";
            TempData["Info"] = "Review appointment impact after changing doctor availability.";
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = schedule.DoctorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.DoctorSchedules.FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            var doctorId = schedule.DoctorId;

            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule deleted successfully.";
            TempData["Info"] = "Review appointment impact after changing doctor availability.";
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageLeaves(int doctorId)
        {
            ViewData["Title"] = "Manage Leaves";

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            var model = new ManageDoctorLeavesViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                Leaves = doctor.Leaves
                    .OrderByDescending(l => l.StartDate)
                    .Select(l => new DoctorLeaveItemViewModel
                    {
                        DoctorLeaveId = l.Id,
                        LeaveStartDate = l.StartDate,
                        LeaveEndDate = l.EndDate,
                        LeaveReason = l.Reason
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddLeave(int doctorId)
        {
            ViewData["Title"] = "Add Leave";

            var exists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);
            if (!exists)
            {
                return NotFound();
            }

            return View(new UpsertDoctorLeaveViewModel
            {
                DoctorId = doctorId,
                LeaveStartDate = DateTime.Today,
                LeaveEndDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLeave(UpsertDoctorLeaveViewModel model)
        {
            ViewData["Title"] = "Add Leave";

            var doctor = await _context.Doctors
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.Id == model.DoctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            ValidateLeaveModel(model);

            if (HasLeaveOverlap(
                doctor.Leaves,
                model.LeaveStartDate,
                model.LeaveEndDate,
                null))
            {
                ModelState.AddModelError(string.Empty, "This leave period overlaps with an existing leave period.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.DoctorLeaves.Add(new DoctorLeave
            {
                DoctorId = model.DoctorId,
                StartDate = model.LeaveStartDate.Date,
                EndDate = model.LeaveEndDate.Date,
                Reason = model.LeaveReason.Trim()
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave added successfully.";
            TempData["Info"] = "Review appointment impact after changing doctor availability.";
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = model.DoctorId });
        }

        [HttpGet]
        public async Task<IActionResult> EditLeave(int id)
        {
            ViewData["Title"] = "Edit Leave";

            var leave = await _context.DoctorLeaves
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null)
            {
                return NotFound();
            }

            return View(new UpsertDoctorLeaveViewModel
            {
                DoctorLeaveId = leave.Id,
                DoctorId = leave.DoctorId,
                LeaveStartDate = leave.StartDate,
                LeaveEndDate = leave.EndDate,
                LeaveReason = leave.Reason
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLeave(UpsertDoctorLeaveViewModel model)
        {
            ViewData["Title"] = "Edit Leave";

            if (!model.DoctorLeaveId.HasValue)
            {
                return NotFound();
            }

            var leave = await _context.DoctorLeaves
                .FirstOrDefaultAsync(l => l.Id == model.DoctorLeaveId.Value);

            if (leave == null)
            {
                return NotFound();
            }

            var existingLeaves = await _context.DoctorLeaves
                .Where(l => l.DoctorId == leave.DoctorId)
                .ToListAsync();

            ValidateLeaveModel(model);

            if (HasLeaveOverlap(
                existingLeaves,
                model.LeaveStartDate,
                model.LeaveEndDate,
                leave.Id))
            {
                ModelState.AddModelError(string.Empty, "This leave period overlaps with an existing leave period.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            leave.StartDate = model.LeaveStartDate.Date;
            leave.EndDate = model.LeaveEndDate.Date;
            leave.Reason = model.LeaveReason.Trim();

            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave updated successfully.";
            TempData["Info"] = "Review appointment impact after changing doctor availability.";
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId = leave.DoctorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            var leave = await _context.DoctorLeaves.FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null)
            {
                return NotFound();
            }

            var doctorId = leave.DoctorId;

            _context.DoctorLeaves.Remove(leave);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave deleted successfully.";
            TempData["Info"] = "Review appointment impact after changing doctor availability.";
            return RedirectToAction(nameof(AppointmentImpact), new { doctorId });
        }

        [HttpGet]
        public async Task<IActionResult> AppointmentImpact(int doctorId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            ViewData["Title"] = "Appointment Impact";

            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .Include(d => d.Leaves)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            var start = fromDate?.Date ?? DateTime.Today;
            var end = toDate?.Date ?? DateTime.Today.AddMonths(1);

            if (end < start)
            {
                end = start;
            }

            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Status)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date >= start &&
                    a.AppointmentDate.Date <= end &&
                    a.Status.Name != "Cancelled" &&
                    a.Status.Name != "Completed" &&
                    a.Status.Name != "Missed")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var impactedAppointments = appointments
                .Where(a => IsAppointmentImpacted(a, doctor.Schedules.ToList(), doctor.Leaves.ToList()))
                .Select(a => new ClinicManagerAppointmentItemViewModel
                {
                    AppointmentId = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = doctor.User.FullName,
                    StatusName = FormatStatusName(a.Status.Name),
                    Notes = a.Notes,
                    ImpactReason = BuildImpactReason(a, doctor.Schedules.ToList(), doctor.Leaves.ToList())
                })
                .ToList();

            var model = new AppointmentImpactViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                FromDate = start,
                ToDate = end,
                ImpactedAppointments = impactedAppointments
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Appointments(int? doctorId = null, string? status = null, DateTime? date = null)
        {
            ViewData["Title"] = "All Appointments";

            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .AsQueryable();

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status.Name == status);
            }

            if (date.HasValue)
            {
                var selectedDate = date.Value.Date;
                query = query.Where(a => a.AppointmentDate.Date == selectedDate);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var doctorOptions = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .OrderBy(d => d.User.FullName)
                .ToListAsync();

            var statusOptions = await _context.AppointmentStatuses
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToListAsync();

            var model = new ClinicManagerAppointmentsViewModel
            {
                SelectedDoctorId = doctorId,
                SelectedStatus = status,
                SelectedDate = date,
                DoctorOptions = doctorOptions.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.User.FullName
                }).ToList(),
                StatusOptions = statusOptions.Select(s => new SelectListItem
                {
                    Value = s.Name,
                    Text = FormatStatusName(s.Name)
                }).ToList(),
                Appointments = appointments.Select(a => new ClinicManagerAppointmentItemViewModel
                {
                    AppointmentId = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    StatusName = FormatStatusName(a.Status.Name),
                    Notes = a.Notes,
                    ImpactReason = null
                }).ToList()
            };

            return View(model);
        }

        private void ValidateScheduleModel(UpsertDoctorScheduleViewModel model)
        {
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(model.EndTime), "End time must be after start time.");
            }

            if (model.SlotDurationMinutes <= 0)
            {
                ModelState.AddModelError(nameof(model.SlotDurationMinutes), "Slot duration must be greater than 0.");
            }
        }

        private bool HasScheduleOverlap(
            IEnumerable<DoctorSchedule> schedules,
            DayOfWeek dayOfWeek,
            TimeOnly startTime,
            TimeOnly endTime,
            int? excludeId)
        {
            return schedules.Any(s =>
                s.DayOfWeek == dayOfWeek &&
                (!excludeId.HasValue || s.Id != excludeId.Value) &&
                startTime < s.EndTime &&
                endTime > s.StartTime);
        }

        private void ValidateLeaveModel(UpsertDoctorLeaveViewModel model)
        {
            if (model.LeaveEndDate.Date < model.LeaveStartDate.Date)
            {
                ModelState.AddModelError(nameof(model.LeaveEndDate), "Leave end date cannot be before leave start date.");
            }

            if (string.IsNullOrWhiteSpace(model.LeaveReason))
            {
                ModelState.AddModelError(nameof(model.LeaveReason), "Leave reason is required.");
            }
        }

        private bool HasLeaveOverlap(
            IEnumerable<DoctorLeave> leaves,
            DateTime startDate,
            DateTime endDate,
            int? excludeId)
        {
            return leaves.Any(l =>
                (!excludeId.HasValue || l.Id != excludeId.Value) &&
                startDate.Date <= l.EndDate.Date &&
                endDate.Date >= l.StartDate.Date);
        }

        private bool IsAppointmentImpacted(
            Appointment appointment,
            List<DoctorSchedule> schedules,
            List<DoctorLeave> leaves)
        {
            if (leaves.Any(l =>
                appointment.AppointmentDate.Date >= l.StartDate.Date &&
                appointment.AppointmentDate.Date <= l.EndDate.Date))
            {
                return true;
            }

            var matchingSchedules = schedules
                .Where(s => s.DayOfWeek == appointment.AppointmentDate.DayOfWeek)
                .ToList();

            if (!matchingSchedules.Any())
            {
                return true;
            }

            var insideAnySchedule = matchingSchedules.Any(s =>
                appointment.StartTime >= s.StartTime &&
                appointment.EndTime <= s.EndTime);

            return !insideAnySchedule;
        }

        private string BuildImpactReason(
            Appointment appointment,
            List<DoctorSchedule> schedules,
            List<DoctorLeave> leaves)
        {
            if (leaves.Any(l =>
                appointment.AppointmentDate.Date >= l.StartDate.Date &&
                appointment.AppointmentDate.Date <= l.EndDate.Date))
            {
                return "Doctor is on leave on this date.";
            }

            var matchingSchedules = schedules
                .Where(s => s.DayOfWeek == appointment.AppointmentDate.DayOfWeek)
                .ToList();

            if (!matchingSchedules.Any())
            {
                return "Doctor has no schedule for this day.";
            }

            var insideAnySchedule = matchingSchedules.Any(s =>
                appointment.StartTime >= s.StartTime &&
                appointment.EndTime <= s.EndTime);

            return insideAnySchedule
                ? string.Empty
                : "Appointment is outside the current working schedule.";
        }

        private string FormatStatusName(string statusName)
        {
            return statusName switch
            {
                "CheckedIn" => "Checked In",
                "InProgress" => "In Progress",
                _ => statusName
            };
        }
    }
}