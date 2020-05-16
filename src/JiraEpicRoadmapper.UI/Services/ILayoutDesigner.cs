using System.Collections.Generic;
using JiraEpicRoadmapper.UI.Models;

namespace JiraEpicRoadmapper.UI.Services
{
    public interface ILayoutDesigner
    {
        IReadOnlyList<IEnumerable<EpicMetadata>> Layout(IEnumerable<EpicMetadata> epics);
    }
}