using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraEpicRoadmapper.Server.Clients
{
    public interface IJiraClient
    {
        Task<IReadOnlyList<JsonElement>> QueryJql(string jqlQuery);
        Task<IReadOnlyDictionary<string, string[]>> QueryFieldNameToKeysMap();
        Task UpdateIssue(string issueKey, IssueContent content);
    }
}
