using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<DoctorLeave> DoctorLeaves { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentStatusLookup> AppointmentStatuses { get; set; }
        public DbSet<VisitRecord> VisitRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DoctorSpecialization>()
                .HasKey(ds => ds.Id);

            modelBuilder.Entity<DoctorSpecialization>()
                .HasIndex(ds => new { ds.DoctorId, ds.SpecializationId })
                .IsUnique()
                .HasDatabaseName("UQ_DoctorSpecialization");

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.NotificationType)
                .WithMany(nt => nt.Notifications)
                .HasForeignKey(n => n.NotificationTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Status)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitRecord>()
                .HasOne(v => v.Appointment)
                .WithOne(a => a.VisitRecord)
                .HasForeignKey<VisitRecord>(v => v.AppointmentId);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.StartTime })
                .IsUnique()
                .HasDatabaseName("UQ_Appointment_Doctor_DateTime");

            modelBuilder.Entity<VisitRecord>()
                .HasIndex(v => v.AppointmentId)
                .IsUnique()
                .HasDatabaseName("UQ_VisitRecord_Appointment");

            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.LicenseNumber)
                .IsUnique()
                .HasDatabaseName("UQ_Doctor_LicenseNumber");

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.CPRNumber)
                .IsUnique()
                .HasDatabaseName("UQ_Patient_CPRNumber");

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.ReferenceNumber)
                .IsUnique()
                .HasDatabaseName("UQ_Patient_ReferenceNumber");

            modelBuilder.Entity<Specialization>()
                .HasIndex(s => s.Name)
                .IsUnique()
                .HasDatabaseName("UQ_Specialization_Name");

            modelBuilder.Entity<AppointmentStatusLookup>()
                .HasIndex(s => s.Name)
                .IsUnique()
                .HasDatabaseName("UQ_AppointmentStatus_Name");

            modelBuilder.Entity<DoctorSchedule>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_DoctorSchedule_Times",
                    "EndTime > StartTime"));

            modelBuilder.Entity<Prescription>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Prescription_Duration",
                    "DurationDays > 0"));

            modelBuilder.Entity<DoctorSchedule>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_DoctorSchedule_SlotDuration",
                    "SlotDurationMinutes > 0"));

            modelBuilder.Entity<AppointmentStatusLookup>().HasData(
                new AppointmentStatusLookup { Id = 1, Name = "Requested", Description = "Appointment has been requested" },
                new AppointmentStatusLookup { Id = 2, Name = "Confirmed", Description = "Appointment has been confirmed" },
                new AppointmentStatusLookup { Id = 3, Name = "CheckedIn", Description = "Patient has checked in" },
                new AppointmentStatusLookup { Id = 4, Name = "InProgress", Description = "Appointment is in progress" },
                new AppointmentStatusLookup { Id = 5, Name = "Completed", Description = "Appointment has been completed" },
                new AppointmentStatusLookup { Id = 6, Name = "Cancelled", Description = "Appointment has been cancelled" },
                new AppointmentStatusLookup { Id = 7, Name = "Missed", Description = "Patient missed the appointment" }
            );

            modelBuilder.Entity<NotificationType>().HasData(
                new NotificationType { Id = 1, Name = "Appointment" },
                new NotificationType { Id = 2, Name = "Prescription" },
                new NotificationType { Id = 3, Name = "General" }
            );
        }
    }
}