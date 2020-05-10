using System.Collections.Generic;
using System.Linq;

namespace JiraEpicRoadmapper.UI.Models
{
    public class LayoutDesigner : ILayoutDesigner
    {
        public IReadOnlyList<IEnumerable<EpicMetadata>> Layout(IEnumerable<EpicMetadata> epics)
        {
            var result = new List<List<EpicMetadata>>();
            foreach (var e in epics.OrderBy(e => e.Start.Index).ThenBy(e => e.Epic.Id))
            {
                var row = result.FirstOrDefault(r => r.Last().End.Index <= e.Start.Index);
                if (row != null) { row.Add(e); }
                else
                {
                    result.Add(new List<EpicMetadata> { e });
                }
            }

            return result;
        }
    }
}