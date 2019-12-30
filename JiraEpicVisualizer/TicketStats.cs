namespace JiraEpicVisualizer
{
    public class TicketStats
    {
        public int Done { get; set; }
        public int InProgress { get; set; }
        public int NotStarted { get; set; }
        public int Total => Done + InProgress + NotStarted;
        public string Percentage => Total > 0 ? $"{Done * 100 / Total}%" : "n/a";
    }
}