using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.UI.Repositories
{
    public interface IEpicsRepository
    {
        Task<Epic[]> FetchEpics();
    }
}
