using JiraEpicRoadmapper.UI.Models;

namespace JiraEpicRoadmapper.UI.Services
{
    public interface IEpicCardPainter
    {
        string GetColor(EpicCard block);
    }
}
