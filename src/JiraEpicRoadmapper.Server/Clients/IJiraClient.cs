using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraEpicRoadmapper.Server.Clients
{
    public interface IJiraClient
    {
        Task<IReadOnlyList<JsonElement>> QueryJql(string jqlQuery);
    }
}
