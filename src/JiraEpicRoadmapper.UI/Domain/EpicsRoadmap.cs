using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Services;

namespace JiraEpicRoadmapper.UI.Domain
{
    public class EpicsRoadmap
    {
        public EpicMap Map { get; }
        public Timeline Timeline { get; }
        public int TotalDays => Timeline.TotalDays;
        public int TotalRows => Projects.LastOrDefault()?.LastRowIndex + 1 ?? 1;
        public IReadOnlyList<ProjectLayout> Projects { get; private set; } = Array.Empty<ProjectLayout>();
        public IEnumerable<EpicCard> EpicCards => Projects.SelectMany(p => p.Epics);
        public event Action OnLayoutUpdate;

        public EpicsRoadmap(IReadOnlyList<Epic> epics, DateTime? today = null)
        {
            Timeline = Timeline.FromEpics(epics, today);
            Map = EpicMap.Create(epics, Timeline);
        }

        public void UpdateLayout(ILayoutDesigner designer, IViewOptions viewOptions)
        {
            var projects = new List<ProjectLayout>();
            var projectGroups = Map.Epics.Where(e => ApplyFilter(e, viewOptions)).GroupBy(e => e.Epic.Project).OrderBy(p => p.Key);
            int row = 1;
            foreach (var projectGroup in projectGroups)
            {
                var projectLayout = ProjectLayout.Create(projectGroup.Key, projectGroup, row + 1, designer);
                projects.Add(projectLayout);
                row = projectLayout.LastRowIndex;
            }
            Projects = projects;

            OnLayoutUpdate?.Invoke();
        }

        private bool ApplyFilter(EpicMetadata epic, IViewOptions viewOptions)
        {
            if (!viewOptions.ShowUnplanned && !epic.Epic.StartDate.HasValue && !epic.Epic.DueDate.HasValue)
                return false;
            if (!viewOptions.ShowClosed && string.Equals(epic.Epic.StatusCategory, "done", StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }
    }
}