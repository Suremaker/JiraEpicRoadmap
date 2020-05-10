namespace JiraEpicRoadmapper.UI.Models
{
    public static class LayoutSettings
    {
        public static readonly int DaySpan = 70;
        public static readonly int TicketHeight = 60;
        public static readonly int RowMargin = 10;
        public static readonly int RowHeight = TicketHeight + 2 * RowMargin;
        public const int MinEpicBlockLength = 4;
    }
}