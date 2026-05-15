using MVCApp.ViewModels.Doctor;

namespace MVCApp.Services.Interfaces
{
    // Service contract for creating and editing doctor visit records.
    public interface IVisitRecordService
    {
        Task<CreateVisitRecordViewModel?> GetCreateVisitRecordAsync(string userId, int appointmentId);
        Task<(bool Success, string? ErrorMessage, int? AppointmentId)> CreateVisitRecordAsync(string userId, CreateVisitRecordViewModel model);
        Task<EditVisitRecordViewModel?> GetEditVisitRecordAsync(string userId, int appointmentId);
        Task<(bool Success, string? ErrorMessage, int? AppointmentId)> EditVisitRecordAsync(string userId, EditVisitRecordViewModel model);
    }
}
