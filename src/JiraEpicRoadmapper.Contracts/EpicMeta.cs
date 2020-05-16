using System;

namespace JiraEpicRoadmapper.Contracts
{
    public class EpicMeta
    {
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
    }
}