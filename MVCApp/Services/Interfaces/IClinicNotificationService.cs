namespace MVCApp.Services.Interfaces
{
    public interface IClinicNotificationService
    {
        Task CreateUserNotificationAsync(
            string userId,
            string title,
            string message,
            string? relatedEntityType = null,
            int? relatedEntityId = null,
            string? notificationTypeName = null);

        Task CreatePatientNotificationAsync(
            int patientId,
            string title,
            string message,
            string? relatedEntityType = null,
            int? relatedEntityId = null,
            string? notificationTypeName = null);

        Task CreateDoctorNotificationAsync(
            int doctorId,
            string title,
            string message,
            string? relatedEntityType = null,
            int? relatedEntityId = null,
            string? notificationTypeName = null);
    }
}