using MVCApp.Services.Interfaces;

namespace MVCApp.Services
{
    // Service that contains the valid appointment lifecycle rules.
    // This keeps status workflow logic outside the controller.
    public class AppointmentWorkflowService : IAppointmentWorkflowService
    {
        private static class StatusNames
        {
            public const string Requested = "Requested";
            public const string Confirmed = "Confirmed";
            public const string CheckedIn = "CheckedIn";
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string Missed = "Missed";
        }

        // Returns only the statuses that are allowed after the current status.
        public List<string> GetAllowedNextStatuses(string currentStatus)
        {
            var normalizedStatus = NormalizeStatusName(currentStatus);

            return normalizedStatus switch
            {
                StatusNames.Requested => new List<string>
                {
                    StatusNames.Confirmed,
                    StatusNames.Cancelled
                },

                StatusNames.Confirmed => new List<string>
                {
                    StatusNames.CheckedIn,
                    StatusNames.Cancelled,
                    StatusNames.Missed
                },

                StatusNames.CheckedIn => new List<string>
                {
                    StatusNames.InProgress,
                    StatusNames.Cancelled
                },

                StatusNames.InProgress => new List<string>
                {
                    StatusNames.Completed,
                    StatusNames.Cancelled
                },

                _ => new List<string>()
            };
        }

        // Checks if changing from currentStatus to newStatus is allowed.
        public bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(currentStatus) || string.IsNullOrWhiteSpace(newStatus))
            {
                return false;
            }

            var normalizedNewStatus = NormalizeStatusName(newStatus);

            return GetAllowedNextStatuses(currentStatus)
                .Contains(normalizedNewStatus);
        }

        // Terminal statuses cannot be updated again.
        public bool CanUpdateStatus(string currentStatus)
        {
            var normalizedStatus = NormalizeStatusName(currentStatus);

            return normalizedStatus != StatusNames.Completed &&
                   normalizedStatus != StatusNames.Cancelled &&
                   normalizedStatus != StatusNames.Missed;
        }

        // Converts database-style status names to readable text for views.
        public string FormatStatusName(string statusName)
        {
            var normalizedStatus = NormalizeStatusName(statusName);

            return normalizedStatus switch
            {
                StatusNames.CheckedIn => "Checked In",
                StatusNames.InProgress => "In Progress",
                _ => normalizedStatus
            };
        }

        // Handles small differences like "Checked In" vs "CheckedIn".
        public string NormalizeStatusName(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
            {
                return string.Empty;
            }

            var cleanStatus = statusName.Trim();

            return cleanStatus switch
            {
                "Checked In" => StatusNames.CheckedIn,
                "In Progress" => StatusNames.InProgress,
                _ => cleanStatus
            };
        }
    }
}