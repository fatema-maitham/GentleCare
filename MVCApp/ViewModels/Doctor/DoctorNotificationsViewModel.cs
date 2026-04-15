using WebAPI.Models;

namespace MVCApp.ViewModels.Doctor
{
    public class DoctorNotificationsViewModel
    {
        public int UnreadCount { get; set; }

        public List<Notification> Notifications { get; set; } = new();
    }
}