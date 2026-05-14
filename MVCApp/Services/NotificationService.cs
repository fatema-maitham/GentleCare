using Microsoft.EntityFrameworkCore;
using MVCApp.Services.Interfaces;
using WebAPI.Data;
using WebAPI.Models;

namespace MVCApp.Services
{
    // Service for notification database logic.
    // Keeping this logic here makes controllers cleaner and avoids repeating code.
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Creates one notification for a specific user.
        public async Task CreateNotificationAsync(
            string userId,
            string title,
            string message,
            string notificationTypeName,
            int? relatedEntityId = null,
            string? relatedEntityType = null)
        {
            // Do not create empty notifications.
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            // Find or create the notification type.
            var notificationType = await GetOrCreateNotificationTypeAsync(notificationTypeName);

            var notification = new Notification
            {
                UserId = userId,
                Title = title.Trim(),
                Message = message.Trim(),
                NotificationTypeId = notificationType.Id,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = string.IsNullOrWhiteSpace(relatedEntityType)
                    ? null
                    : relatedEntityType.Trim(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // Creates the same notification for multiple users.
        public async Task CreateNotificationsAsync(
            IEnumerable<string> userIds,
            string title,
            string message,
            string notificationTypeName,
            int? relatedEntityId = null,
            string? relatedEntityType = null)
        {
            var cleanUserIds = userIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (!cleanUserIds.Any() ||
                string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var notificationType = await GetOrCreateNotificationTypeAsync(notificationTypeName);

            var notifications = cleanUserIds.Select(userId => new Notification
            {
                UserId = userId,
                Title = title.Trim(),
                Message = message.Trim(),
                NotificationTypeId = notificationType.Id,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = string.IsNullOrWhiteSpace(relatedEntityType)
                    ? null
                    : relatedEntityType.Trim(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }

        // Gets all notifications for one user, newest first.
        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new List<Notification>();
            }

            return await _context.Notifications
                .AsNoTracking()
                .Include(n => n.NotificationType)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // Marks one notification as read.
        // Returns false if the notification does not exist or does not belong to the user.
        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return true;
        }

        // Marks all notifications for one user as read.
        public async Task MarkAllAsReadAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!unreadNotifications.Any())
            {
                return;
            }

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        // Counts unread notifications for one user.
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return 0;
            }

            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        // Finds a notification type by name.
        // If it does not exist, it creates it.
        private async Task<NotificationType> GetOrCreateNotificationTypeAsync(string notificationTypeName)
        {
            var cleanTypeName = string.IsNullOrWhiteSpace(notificationTypeName)
                ? "General"
                : notificationTypeName.Trim();

            var notificationType = await _context.NotificationTypes
                .FirstOrDefaultAsync(t => t.Name == cleanTypeName);

            if (notificationType != null)
            {
                return notificationType;
            }

            notificationType = new NotificationType
            {
                Name = cleanTypeName
            };

            _context.NotificationTypes.Add(notificationType);
            await _context.SaveChangesAsync();

            return notificationType;
        }
    }
}