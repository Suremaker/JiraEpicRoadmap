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
        public EpicMap EpicMap { get; }
        public IReadOnlyList<ProjectLayout> Projects { get; }
        public int TotalRows { get; }
        public int MaxIndex { get; }
        public DateTimeOffset Today { get; } = DateTimeOffset.Now.Date;

        public Timeline(Epic[] epics)
        {
            Start = epics.Select(e => e.StartDate.GetValueOrDefault(Today)).DefaultIfEmpty(Today).Min().AddDays(-7);
            End = epics.Select(e => e.DueDate.GetValueOrDefault(Today)).DefaultIfEmpty(Today).Max().AddMonths(1).GetFirstOfMonth();
            MaxIndex = GetDayIndex(End);
            EpicMap = new EpicMap(epics, GetDayIndex);

            Projects = LayoutProjects();
            TotalRows = Projects.Last().LastRowIndex + 1;
        }

        private IReadOnlyList<ProjectLayout> LayoutProjects()
        {
            var row = 1;
            var projects = new List<ProjectLayout>();
            foreach (var group in EpicMap.Epics.GroupBy(e => e.Epic.Project).OrderBy(e => e.Key))
            {
                var project = new ProjectLayout(group.Key, row + 1, group);
                projects.Add(project);
                row = project.LastRowIndex;
            }
            return projects;
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
    }
}