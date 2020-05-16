using System.Collections.Generic;

namespace JiraEpicRoadmapper.Server.Clients
{
    public class IssueContent
    {
        public IDictionary<string, string> Fields { get; } = new Dictionary<string, string>();
    }
}