using System.Collections.Generic;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.Server.Providers
{
    public interface IEpicProvider
    {
        Task<IEnumerable<Epic>> GetEpics();
    }
}
