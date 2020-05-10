using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.UI.Models
{
    public class EpicsRoadmap
    {
        public EpicMap Map { get; }
        public Timeline Timeline { get; }
        public int TotalDays => Timeline.TotalDays;
        public int TotalRows => Projects.LastOrDefault()?.LastRowIndex + 1 ?? 1;
        public IReadOnlyList<ProjectLayout> Projects { get; private set; } = Array.Empty<ProjectLayout>();

        public EpicsRoadmap(IReadOnlyList<Epic> epics, DateTimeOffset? today = null)
        {
            Timeline = Timeline.FromEpics(epics, today);
            Map = EpicMap.Create(epics, Timeline);
        }

        public void UpdateLayout(ILayoutDesigner designer)
        {
            var projects = new List<ProjectLayout>();
            var projectGroups = Map.Epics.GroupBy(e => e.Epic.Project).OrderBy(p => p.Key);
            int row = 1;
            foreach (var projectGroup in projectGroups)
            {
                var projectLayout = ProjectLayout.Create(projectGroup.Key, projectGroup, row + 1, designer);
                projects.Add(projectLayout);
                row = projectLayout.LastRowIndex;
            }
            Projects = projects;
        }
    }
}