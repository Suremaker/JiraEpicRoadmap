namespace JiraEpicRoadmapper.Contracts
{
    public class EpicStats
    {
        public int Done { get; set; }
        public int InProgress { get; set; }
        public int NotStarted { get; set; }
        public int Total => Done + InProgress + NotStarted;

        public override string ToString() => $"{nameof(Total)}: {Total}, {nameof(Done)}: {Done}, {nameof(InProgress)}: {InProgress}, {nameof(NotStarted)}: {NotStarted}";
    }
}