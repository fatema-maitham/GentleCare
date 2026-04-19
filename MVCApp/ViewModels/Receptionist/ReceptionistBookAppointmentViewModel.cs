using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistBookAppointmentViewModel
    {
        [Required(ErrorMessage = "Please select a patient.")]
        [Display(Name = "Patient")]
        public int? PatientId { get; set; }

        [Required(ErrorMessage = "Please select a specialization.")]
        [Display(Name = "Specialization")]
        public int? SpecializationId { get; set; }

        [Required(ErrorMessage = "Please select a doctor.")]
        [Display(Name = "Doctor")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "Please select an appointment date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        public DateTime? AppointmentDate { get; set; }

        [Required(ErrorMessage = "Please select a time slot.")]
        [Display(Name = "Available Time Slots")]
        public string SelectedStartTime { get; set; } = string.Empty;

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public List<SelectListItem> Patients { get; set; } = new();
        public List<SelectListItem> Specializations { get; set; } = new();
        public List<SelectListItem> Doctors { get; set; } = new();
        public List<SelectListItem> AvailableSlots { get; set; } = new();
    }
}