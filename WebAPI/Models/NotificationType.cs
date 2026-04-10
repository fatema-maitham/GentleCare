namespace WebAPI.Models
{
    public class NotificationType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        
        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();
    }
}