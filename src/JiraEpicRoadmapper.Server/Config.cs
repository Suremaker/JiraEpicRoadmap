namespace JiraEpicRoadmapper.Server
{
    public class Config
    {
        public string JiraUri { get; set; }
        public string AuthKey { get; set; }
        public int JiraQueryThroughput { get; set; } = 10;
        public string EpicQueryFilter { get; set; }
    }
}