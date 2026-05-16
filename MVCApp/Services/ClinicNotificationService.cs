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
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return;
            }

            await CreateUserNotificationAsync(
                patient.UserId,
                title,
                message,
                relatedEntityType,
                relatedEntityId,
                notificationTypeName);
        }
    }
}