using System.Collections.Generic;

namespace JiraEpicRoadmapper.UI.Models
{
    public interface ILayoutDesigner
    {
        IReadOnlyList<IEnumerable<EpicMetadata>> Layout(IEnumerable<EpicMetadata> epics);
    }
}