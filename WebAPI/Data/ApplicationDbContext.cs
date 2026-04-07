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
        public DbSet<VisitRecord> VisitRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<DoctorSpecialization>()
                .HasKey(ds => new { ds.DoctorId, ds.SpecializationId });

            
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

            modelBuilder.Entity<VisitRecord>()
                .HasOne(v => v.Appointment)
                .WithOne(a => a.VisitRecord)
                .HasForeignKey<VisitRecord>(v => v.AppointmentId);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>();

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
        }
    }
}