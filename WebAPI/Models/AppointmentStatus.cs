namespace WebAPI.Models
{
    public enum AppointmentStatus
    {
        Requested = 0,
        Confirmed = 1,
        CheckedIn = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5,
        Missed = 6
    }
}