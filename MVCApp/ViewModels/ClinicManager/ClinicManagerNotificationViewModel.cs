namespace MVCApp.ViewModels.ClinicManager
{
    // ViewModel used by the Clinic Manager notifications page.
    public class ClinicManagerNotificationViewModel
    {
        public List<ClinicManagerNotificationItemViewModel> Notifications { get; set; } = new();

        // Summary values
        public int TotalNotifications => Notifications.Count;

        public int UnreadNotifications => Notifications.Count(n => !n.IsRead);

        public int ReadNotifications => Notifications.Count(n => n.IsRead);

        // View helpers
        public bool HasNotifications => Notifications.Any();

        public bool HasUnreadNotifications => Notifications.Any(n => !n.IsRead);

        public string EmptyMessage => "No notifications are available.";
    }

    // One notification row/card shown to the Clinic Manager.
    public class ClinicManagerNotificationItemViewModel
    {
        public int NotificationId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string NotificationTypeName { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? RelatedEntityId { get; set; }

        public string RelatedEntityType { get; set; } = string.Empty;

        // View helper text
        public string StatusText => IsRead ? "Read" : "Unread";

        public string CreatedAtText => CreatedAt.ToString("dd MMM yyyy, HH:mm");

        public bool HasRelatedEntity =>
            RelatedEntityId.HasValue &&
            !string.IsNullOrWhiteSpace(RelatedEntityType);
    }
}