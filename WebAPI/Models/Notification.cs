namespace WebAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public int? NotificationTypeId { get; set; }  
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public ApplicationUser User { get; set; } = null!;
        public NotificationType? NotificationType { get; set; }
    }
}