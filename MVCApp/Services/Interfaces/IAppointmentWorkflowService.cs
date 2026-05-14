namespace MVCApp.Services.Interfaces
{
    // Service interface for appointment lifecycle rules.
    // It controls which appointment status changes are allowed.
    public interface IAppointmentWorkflowService
    {
        List<string> GetAllowedNextStatuses(string currentStatus);

        bool IsValidStatusTransition(string currentStatus, string newStatus);

        bool CanUpdateStatus(string currentStatus);

        string FormatStatusName(string statusName);

        string NormalizeStatusName(string statusName);
    }
}