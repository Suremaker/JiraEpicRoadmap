using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Models;

namespace JiraEpicRoadmapper.Client.Model
{
    public class Timeline
    {
        public DateTimeOffset Start { get; }
        public DateTimeOffset End { get; }
        private EpicMap EpicMap { get; }
        public IReadOnlyList<ProjectLayout> Projects { get; private set; } = Array.Empty<ProjectLayout>();
        public int TotalRows => (Projects.LastOrDefault()?.LastRowIndex ?? 0) + 1;
        public int MaxIndex { get; }
        public DateTimeOffset Today { get; } = DateTimeOffset.Now.Date;
        public IEnumerable<EpicBlock> Epics => EpicMap.Epics.Where(e => e.IsVisible);

        public Timeline(Epic[] epics)
        {
            Start = epics.Select(e => e.StartDate.GetValueOrDefault(Today)).DefaultIfEmpty(Today).Min().AddDays(-7);
            End = epics.Select(e => e.DueDate.GetValueOrDefault(Today)).DefaultIfEmpty(Today).Max().AddMonths(1).GetFirstOfMonth();
            MaxIndex = GetDayIndex(End);
            EpicMap = new EpicMap(epics, GetDayIndex);
        }

        public IEnumerable<(DateTimeOffset day, int index)> GetMondays()
        {
            var day = Start;
            while (day.DayOfWeek != DayOfWeek.Monday)
                day = day.AddDays(1);
            while (day < End)
            {
                yield return GetDayWithIndex(day);
                day = day.AddDays(7);
            }
        }

        public IEnumerable<(DateTimeOffset day, int index)> GetToday()
        {
            if (Today >= Start && Today <= End)
                yield return GetDayWithIndex(Today);
        }

        private (DateTimeOffset day, int index) GetDayWithIndex(in DateTimeOffset day) => (day, GetDayIndex(day));
        private int GetDayIndex(DateTimeOffset? day) => GetDayIndex(day.GetValueOrDefault(Today));
        private int GetDayIndex(DateTimeOffset day) => (int)(day - Start).TotalDays;

        public void UpdateLayout(bool showDependencies, bool hideClosedEpics)
        {
            var row = 1;
            var projects = new List<ProjectLayout>();
            foreach (var group in EpicMap.Epics.Where(e => FilterEpic(e, hideClosedEpics)).GroupBy(e => e.Epic.Project).OrderBy(e => e.Key))
            {
                var project = new ProjectLayout(group.Key, row + 1, group, showDependencies);
                projects.Add(project);
                row = project.LastRowIndex;
            }

            Projects = projects;
        }

        private bool FilterEpic(EpicBlock epic, in bool hideClosedEpics)
        {
            var visible = !hideClosedEpics || !string.Equals(epic.Epic.Status, "done", StringComparison.OrdinalIgnoreCase);
            epic.IsVisible = visible;
            return visible;
        }
    }
}