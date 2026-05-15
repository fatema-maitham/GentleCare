using WebAPI.Models;

namespace MVCApp.Services.Interfaces
{
    // Service interface for creating and managing in-system notifications.
    // It is used when appointment status changes, appointments are cancelled,
    // or doctor schedule/leave changes affect users.
    public interface INotificationService
    {
        // Creates one notification for one user.
        Task CreateNotificationAsync(
            string userId,
            string title,
            string message,
            string notificationTypeName,
            int? relatedEntityId = null,
            string? relatedEntityType = null);

        // Creates the same notification for many users.
        Task CreateNotificationsAsync(
            IEnumerable<string> userIds,
            string title,
            string message,
            string notificationTypeName,
            int? relatedEntityId = null,
            string? relatedEntityType = null);

        // Gets notifications for one user.
        Task<List<Notification>> GetUserNotificationsAsync(string userId);

        // Marks one notification as read.
        Task<bool> MarkAsReadAsync(int notificationId, string userId);

        // Marks all notifications as read for one user.
        Task MarkAllAsReadAsync(string userId);

        // Gets unread notification count for one user.
        Task<int> GetUnreadCountAsync(string userId);
    }
}