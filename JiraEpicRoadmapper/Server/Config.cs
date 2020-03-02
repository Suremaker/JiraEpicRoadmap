namespace JiraEpicRoadmapper.Server
{
    public class Config
    {
        public string JiraUri { get; set; }
        public string AuthKey { get; set; }
        public string[] ProjectFilters { get; set; }
        public string EpicQueryFilter{get;set;}
    }
}
