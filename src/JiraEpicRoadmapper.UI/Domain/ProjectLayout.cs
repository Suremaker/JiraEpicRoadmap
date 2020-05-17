using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Services;

namespace JiraEpicRoadmapper.UI.Domain
{
    public class ProjectLayout
    {
        public string Name { get; }
        public int ProjectRowIndex { get; }
        public IReadOnlyList<EpicCard> Epics { get; }
        public int LastRowIndex { get; }

        private ProjectLayout(string name, IReadOnlyList<EpicCard> epics, int projectRowIndex, int lastRowIndex)
        {
            Name = name;
            Epics = epics;
            ProjectRowIndex = projectRowIndex;
            LastRowIndex = lastRowIndex;
        }

        public static ProjectLayout Create(
            string name, IEnumerable<EpicMetadata> epics,
            int projectRowIndex, ILayoutDesigner designer)
        {
            var rows = designer.Layout(epics);
            var cards = rows
                .SelectMany((row, index) => row.Select(m => new EpicCard(m, index + projectRowIndex + 1)))
                .ToArray();
            return new ProjectLayout(name, cards, projectRowIndex, projectRowIndex + rows.Count);
        }
    }
}