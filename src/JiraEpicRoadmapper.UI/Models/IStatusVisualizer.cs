using System.Collections.Generic;

namespace JiraEpicRoadmapper.UI.Models
{
    public interface IStatusVisualizer
    {
        IEnumerable<string> GetStatusIcons(EpicVisualBlock block);
    }
}