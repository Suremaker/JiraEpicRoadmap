using System;

namespace JiraEpicVisualizer
{
    class Epic
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Project { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
        public Link[] Links { get; set; }
        public TicketStats Stats { get; set; }
        public DateTimeOffset? DueDate { get; set; }
    }
}
