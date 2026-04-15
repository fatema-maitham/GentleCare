using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCApp.ViewModels.Receptionist;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Controllers
{
    [Authorize(Roles = "Receptionist")]
    public class ReceptionistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceptionistController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var todayAppointmentsQuery = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .Where(a => a.AppointmentDate.Date == today);

            var model = new ReceptionistDashboardViewModel
            {
                TodayTotalAppointments = await todayAppointmentsQuery.CountAsync(),
                TodayConfirmedCount = await todayAppointmentsQuery.CountAsync(a => a.Status.Name == "Confirmed"),
                TodayCheckedInCount = await todayAppointmentsQuery.CountAsync(a => a.Status.Name == "CheckedIn"),
                TodayInProgressCount = await todayAppointmentsQuery.CountAsync(a => a.Status.Name == "InProgress"),
                TodayCompletedCount = await todayAppointmentsQuery.CountAsync(a => a.Status.Name == "Completed"),
                TodayAppointments = await todayAppointmentsQuery
                    .OrderBy(a => a.StartTime)
                    .Select(a => new ReceptionistAppointmentListItemViewModel
                    {
                        AppointmentId = a.Id,
                        PatientName = a.Patient.User.FullName,
                        CPRNumber = a.Patient.CPRNumber,
                        ReferenceNumber = a.Patient.ReferenceNumber,
                        DoctorName = a.Doctor.User.FullName,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime.ToString("HH:mm"),
                        EndTime = a.EndTime.ToString("HH:mm"),
                        Status = a.Status.Name,
                        Notes = a.Notes,
                        CancellationReason = a.CancellationReason
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Appointments(string? searchText, DateTime? selectedDate, string? selectedStatus)
        {
            var model = new ReceptionistAppointmentsPageViewModel
            {
                SearchText = searchText,
                SelectedDate = selectedDate
            };

            model.StatusOptions = await _context.AppointmentStatuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Name,
                    Text = s.Name
                })
                .ToListAsync();

            var query = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim();

                query = query.Where(a =>
                    a.Patient.User.FullName.Contains(search) ||
                    a.Patient.CPRNumber.Contains(search) ||
                    a.Patient.ReferenceNumber.Contains(search) ||
                    a.Doctor.User.FullName.Contains(search));
            }

            if (selectedDate.HasValue)
            {
                var date = selectedDate.Value.Date;
                query = query.Where(a => a.AppointmentDate.Date == date);
            }

            if (!string.IsNullOrWhiteSpace(selectedStatus))
            {
                query = query.Where(a => a.Status.Name == selectedStatus);
            }

            model.Appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new ReceptionistAppointmentListItemViewModel
                {
                    AppointmentId = a.Id,
                    PatientName = a.Patient.User.FullName,
                    CPRNumber = a.Patient.CPRNumber,
                    ReferenceNumber = a.Patient.ReferenceNumber,
                    DoctorName = a.Doctor.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString("HH:mm"),
                    EndTime = a.EndTime.ToString("HH:mm"),
                    Status = a.Status.Name,
                    Notes = a.Notes,
                    CancellationReason = a.CancellationReason
                })
                .ToListAsync();

            ViewBag.SelectedStatus = selectedStatus;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment()
        {
            var model = new ReceptionistBookAppointmentViewModel
            {
                AppointmentDate = DateTime.Today.AddDays(1)
            };

            await PopulateBookAppointmentListsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(ReceptionistBookAppointmentViewModel model, string command)
        {
            await PopulateBookAppointmentListsAsync(model);

            if (command == "load")
            {
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!model.PatientId.HasValue)
            {
                ModelState.AddModelError(nameof(model.PatientId), "Please select a patient.");
                return View(model);
            }

            if (!model.SpecializationId.HasValue)
            {
                ModelState.AddModelError(nameof(model.SpecializationId), "Please select a specialization.");
                return View(model);
            }

            if (!model.DoctorId.HasValue)
            {
                ModelState.AddModelError(nameof(model.DoctorId), "Please select a doctor.");
                return View(model);
            }

            if (!model.AppointmentDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.AppointmentDate), "Please select an appointment date.");
                return View(model);
            }

            if (model.AppointmentDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.AppointmentDate), "Appointment date cannot be in the past.");
                return View(model);
            }

            if (!TimeOnly.TryParse(model.SelectedStartTime, out var selectedStartTime))
            {
                ModelState.AddModelError(nameof(model.SelectedStartTime), "Please select a valid time slot.");
                return View(model);
            }

            var doctorId = model.DoctorId.Value;
            var specializationId = model.SpecializationId.Value;
            var patientId = model.PatientId.Value;
            var appointmentDate = model.AppointmentDate.Value.Date;

            var doctorHasSpecialization = await _context.DoctorSpecializations
                .AnyAsync(ds => ds.DoctorId == doctorId && ds.SpecializationId == specializationId);

            if (!doctorHasSpecialization)
            {
                ModelState.AddModelError(nameof(model.DoctorId), "Selected doctor does not match the selected specialization.");
                return View(model);
            }

            var schedule = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId
                         && s.DayOfWeek == appointmentDate.DayOfWeek
                         && s.StartTime <= selectedStartTime)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                ModelState.AddModelError(nameof(model.SelectedStartTime), "Selected time does not match doctor schedule.");
                return View(model);
            }

            var endTime = selectedStartTime.AddMinutes(schedule.SlotDurationMinutes);

            if (endTime > schedule.EndTime)
            {
                ModelState.AddModelError(nameof(model.SelectedStartTime), "Selected time exceeds doctor schedule.");
                return View(model);
            }

            var hasLeave = await _context.DoctorLeaves.AnyAsync(l =>
                l.DoctorId == doctorId &&
                appointmentDate >= l.StartDate.Date &&
                appointmentDate <= l.EndDate.Date);

            if (hasLeave)
            {
                ModelState.AddModelError(nameof(model.AppointmentDate), "Doctor is on leave for the selected date.");
                return View(model);
            }

            var hasConflict = await _context.Appointments
                .Include(a => a.Status)
                .AnyAsync(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date == appointmentDate &&
                    a.Status.Name != "Cancelled" &&
                    selectedStartTime < a.EndTime &&
                    a.StartTime < endTime);

            if (hasConflict)
            {
                ModelState.AddModelError(nameof(model.SelectedStartTime), "This slot is already booked.");
                return View(model);
            }

            var confirmedStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Confirmed");

            if (confirmedStatus == null)
            {
                ModelState.AddModelError(string.Empty, "Confirmed status was not found in the database.");
                return View(model);
            }

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                StatusId = confirmedStatus.Id,
                AppointmentDate = appointmentDate,
                StartTime = selectedStartTime,
                EndTime = endTime,
                Notes = model.Notes,
                CancellationReason = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully.";
            return RedirectToAction(nameof(Appointments));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            var allowedStatusNames = GetAllowedNextStatuses(appointment.Status.Name);

            var model = new ReceptionistUpdateAppointmentStatusViewModel
            {
                AppointmentId = appointment.Id,
                PatientName = appointment.Patient.User.FullName,
                DoctorName = appointment.Doctor.User.FullName,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime.ToString("HH:mm"),
                EndTime = appointment.EndTime.ToString("HH:mm"),
                CurrentStatus = appointment.Status.Name,
                AllowedStatuses = allowedStatusNames
                    .Select(s => new SelectListItem
                    {
                        Value = s,
                        Text = s
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(ReceptionistUpdateAppointmentStatusViewModel model)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == model.AppointmentId);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            var allowedStatusNames = GetAllowedNextStatuses(appointment.Status.Name);

            model.PatientName = appointment.Patient.User.FullName;
            model.DoctorName = appointment.Doctor.User.FullName;
            model.AppointmentDate = appointment.AppointmentDate;
            model.StartTime = appointment.StartTime.ToString("HH:mm");
            model.EndTime = appointment.EndTime.ToString("HH:mm");
            model.CurrentStatus = appointment.Status.Name;
            model.AllowedStatuses = allowedStatusNames
                .Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s
                })
                .ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!allowedStatusNames.Contains(model.NewStatus))
            {
                ModelState.AddModelError(nameof(model.NewStatus), "Invalid status transition.");
                return View(model);
            }

            if (model.NewStatus == "Cancelled" && string.IsNullOrWhiteSpace(model.CancellationReason))
            {
                ModelState.AddModelError(nameof(model.CancellationReason), "Cancellation reason is required when cancelling an appointment.");
                return View(model);
            }

            var newStatus = await _context.AppointmentStatuses
                .FirstOrDefaultAsync(s => s.Name == model.NewStatus);

            if (newStatus == null)
            {
                ModelState.AddModelError(nameof(model.NewStatus), "Selected status was not found.");
                return View(model);
            }

            appointment.StatusId = newStatus.Id;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (model.NewStatus == "Cancelled")
            {
                appointment.CancellationReason = model.CancellationReason;
            }
            else
            {
                appointment.CancellationReason = null;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment status updated successfully.";
            return RedirectToAction(nameof(Appointments));
        }

        private async Task PopulateBookAppointmentListsAsync(ReceptionistBookAppointmentViewModel model)
        {
            model.Patients = await _context.Patients
                .Include(p => p.User)
                .OrderBy(p => p.User.FullName)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.User.FullName + " - " + p.CPRNumber
                })
                .ToListAsync();

            model.Specializations = await _context.Specializations
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            model.Doctors = new List<SelectListItem>();
            model.AvailableSlots = new List<SelectListItem>();

            if (model.SpecializationId.HasValue)
            {
                var doctorRows = await _context.DoctorSpecializations
                    .Where(ds => ds.SpecializationId == model.SpecializationId.Value)
                    .Include(ds => ds.Doctor)
                        .ThenInclude(d => d.User)
                    .Select(ds => new
                    {
                        ds.DoctorId,
                        DoctorName = ds.Doctor.User.FullName
                    })
                    .Distinct()
                    .OrderBy(x => x.DoctorName)
                    .ToListAsync();

                model.Doctors = doctorRows
                    .Select(d => new SelectListItem
                    {
                        Value = d.DoctorId.ToString(),
                        Text = d.DoctorName
                    })
                    .ToList();
            }

            if (model.DoctorId.HasValue && model.AppointmentDate.HasValue)
            {
                model.AvailableSlots = await BuildAvailableSlotsAsync(model.DoctorId.Value, model.AppointmentDate.Value.Date);
            }
        }

        private async Task<List<SelectListItem>> BuildAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var slots = new List<SelectListItem>();

            var hasLeave = await _context.DoctorLeaves.AnyAsync(l =>
                l.DoctorId == doctorId &&
                date.Date >= l.StartDate.Date &&
                date.Date <= l.EndDate.Date);

            if (hasLeave)
            {
                return slots;
            }

            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == date.DayOfWeek)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var existingAppointments = await _context.Appointments
                .Include(a => a.Status)
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDate.Date == date.Date
                         && a.Status.Name != "Cancelled")
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                var current = schedule.StartTime;

                while (current.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
                {
                    var slotStart = current;
                    var slotEnd = current.AddMinutes(schedule.SlotDurationMinutes);

                    var overlaps = existingAppointments.Any(a =>
                        slotStart < a.EndTime && a.StartTime < slotEnd);

                    if (!overlaps)
                    {
                        slots.Add(new SelectListItem
                        {
                            Value = slotStart.ToString("HH:mm"),
                            Text = $"{slotStart.ToString("hh:mm tt")} - {slotEnd.ToString("hh:mm tt")}"
                        });
                    }

                    current = current.AddMinutes(schedule.SlotDurationMinutes);
                }
            }

            return slots;
        }

        private static List<string> GetAllowedNextStatuses(string currentStatus)
        {
            return currentStatus switch
            {
                "Requested" => new List<string> { "Confirmed", "Cancelled" },
                "Confirmed" => new List<string> { "CheckedIn", "Cancelled" },
                "CheckedIn" => new List<string> { "InProgress" },
                "InProgress" => new List<string> { "Completed", "Missed" },
                _ => new List<string>()
            };
        }
    }
}