using Microsoft.AspNetCore.SignalR;
using WebAPI.Hubs;

namespace WebAPI.Services
{
    public class NotificationHubService
    {
        private readonly IHubContext<AppointmentHub> _hubContext;

        public NotificationHubService(
            IHubContext<AppointmentHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyAppointmentStatusChanged(
            int appointmentId,
            string patientName,
            string doctorName,
            string newStatus)
        {
            await _hubContext.Clients
                .Group("ReceptionistGroup")
                .SendAsync("AppointmentStatusChanged", new
                {
                    AppointmentId = appointmentId,
                    PatientName = patientName,
                    DoctorName = doctorName,
                    NewStatus = newStatus,
                    UpdatedAt = DateTime.UtcNow
                });
        }

        public async Task NotifyPatient(
            int patientId,
            string title,
            string message)
        {
            await _hubContext.Clients
                .Group($"Patient_{patientId}")
                .SendAsync("ReceiveNotification", new
                {
                    Title = title,
                    Message = message,
                    CreatedAt = DateTime.UtcNow
                });
        }

        public async Task NotifyDoctor(
            int doctorId,
            string title,
            string message)
        {
            await _hubContext.Clients
                .Group($"Doctor_{doctorId}")
                .SendAsync("ReceiveNotification", new
                {
                    Title = title,
                    Message = message,
                    CreatedAt = DateTime.UtcNow
                });
        }

        public async Task UpdateWaitingRoom(object queueData)
        {
            await _hubContext.Clients
                .Group("ReceptionistGroup")
                .SendAsync("WaitingRoomUpdated", queueData);
        }
    }
}