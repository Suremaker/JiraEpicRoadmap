using System;

namespace JiraEpicRoadmapper.Models
{
    public class EpicMeta
    {
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
    }
}