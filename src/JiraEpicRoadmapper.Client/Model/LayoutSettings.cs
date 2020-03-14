namespace JiraEpicRoadmapper.Client.Model
{
    public static class LayoutSettings
    {
        public const int MinBlockLength = 3;
        public static readonly int DaySpan = 60;
        public static readonly int CellMargin = 10;
        public static readonly int TicketHeight = 50;
        public static readonly int RowMargin = 10;
        public static readonly int RowHeight = TicketHeight + 2 * RowMargin;
    }
}