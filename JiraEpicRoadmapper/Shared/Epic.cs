using System;
using System.Linq;

namespace JiraEpicRoadmapper.Shared
{
    public class Epic
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Project { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
        public Link[] Links { get; set; }
        public TicketStats Stats { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset CalculatedStartDate { get; set; }
        public DateTimeOffset CalculatedDueDate { get; set; }
        public string Url { get; set; }

        public bool Overlaps(Epic e) => !(CalculatedDueDate <= e.CalculatedStartDate || e.CalculatedDueDate <= CalculatedStartDate);
        public bool DependsOn(Epic e)=>e.Links.Any(l=>l.OutwardId==Id);
    }
}
