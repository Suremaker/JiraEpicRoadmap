namespace JiraEpicRoadmapper.Models
{
    public class TicketStats
    {
        public int Done { get; set; }
        public int InProgress { get; set; }
        public int NotStarted { get; set; }
        public int Total => Done + InProgress + NotStarted;
        public int InProgressPercentage => Total == 0 ? 0 : InProgress * 100 / Total;
        public int DonePercentage => Total == 0 ? 0 : Done * 100 / Total;
        public int NotStartedPercentage => 100 - DonePercentage - InProgressPercentage;
    }
}