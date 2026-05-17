using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Services
{
    public class ClinicNotificationService : IClinicNotificationService
    {
        private readonly ApplicationDbContext _context;

        public ClinicNotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserNotificationAsync(
            string userId,
            string title,
            string message,
            string? relatedEntityType = null,
            int? relatedEntityId = null,
            string? notificationTypeName = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            int? notificationTypeId = null;

            if (!string.IsNullOrWhiteSpace(notificationTypeName))
            {
                notificationTypeId = await _context.NotificationTypes
                    .Where(t => t.Name == notificationTypeName)
                    .Select(t => (int?)t.Id)
                    .FirstOrDefaultAsync();
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                NotificationTypeId = notificationTypeId,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreatePatientNotificationAsync(
            int patientId,
            string title,
            string message,
            string? relatedEntityType = null,
            int? relatedEntityId = null,
            string? notificationTypeName = null)
        {
            var patientUserId = await _context.Patients
                .Where(p => p.Id == patientId)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(patientUserId))
            {
                return;
            }

            await CreateUserNotificationAsync(
                patientUserId,
                title,
                message,
                relatedEntityType,
                relatedEntityId,
                notificationTypeName);
        }

        public async Task CreateDoctorNotificationAsync(
            int doctorId,
            string title,
            string message,
            string? relatedEntityType = null,
            int? relatedEntityId = null,
            string? notificationTypeName = null)
        {
            var doctorUserId = await _context.Doctors
                .Where(d => d.Id == doctorId)
                .Select(d => d.UserId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(doctorUserId))
            {
                return;
            }

            await CreateUserNotificationAsync(
                doctorUserId,
                title,
                message,
                relatedEntityType,
                relatedEntityId,
                notificationTypeName);
        }
    }
}