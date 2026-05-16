using System.Collections.Generic;

namespace MVCApp.ViewModels.Receptionist
{
    public class ReceptionistLiveQueueViewModel
    {
        public int ConfirmedCount { get; set; }

        public int CheckedInCount { get; set; }

        public int InProgressCount { get; set; }

        public int CompletedCount { get; set; }

        public List<ReceptionistLiveQueueItemViewModel> QueueItems { get; set; } = new();
    }
}