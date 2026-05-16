namespace MVCApp.ViewModels.Doctor
{
    // ViewModel used by the Doctor Notifications page.
    // It contains the unread count and all notifications for the logged-in doctor.
    public class DoctorNotificationsViewModel
    {
        // Number of notifications that are not read yet.
        public int UnreadCount { get; set; }

        // All notifications shown on the page.
        public List<DoctorNotificationViewModel> Notifications { get; set; } = new();

        // Display helpers for the view.
        public bool HasNotifications => Notifications.Any();
        public bool HasUnreadNotifications => UnreadCount > 0;
        public int TotalNotifications => Notifications.Count;
    }

    // Represents one notification row/card for the doctor.
    public class DoctorNotificationViewModel
    {
        // Notification primary key used for mark-as-read actions.
        public int NotificationId { get; set; }

        // Notification content.
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Shows whether the doctor already opened/read the notification.
        public bool IsRead { get; set; }

        // Date and time the notification was created.
        public DateTime CreatedAt { get; set; }

        // Optional link information if the notification is connected to an appointment or prescription.
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }

        // Display helpers for the view.
        public string CreatedAtDisplay => CreatedAt.ToString("dd MMM yyyy HH:mm");
        public string ReadStatusDisplay => IsRead ? "Read" : "Unread";
        public bool HasRelatedEntity => RelatedEntityId.HasValue && !string.IsNullOrWhiteSpace(RelatedEntityType);
    }
}