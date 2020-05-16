using System.Collections.Generic;
using JiraEpicRoadmapper.UI.Models;

namespace JiraEpicRoadmapper.UI.Services
{
    public interface IStatusVisualizer
    {
        IEnumerable<string> GetStatusIcons(EpicCard block);
    }
}