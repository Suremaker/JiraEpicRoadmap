using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Shared;

namespace JiraEpicRoadmapper.Client
{
    public class Timeline
    {
        public DateTimeOffset Start { get; }
        public DateTimeOffset End { get; }
        public ProjectLayout[] Projects { get; }
        public int Rows { get; }
        public IReadOnlyList<(Epic epic, int startIndex, int endIndex, int row)> Epics { get; }
        public int MaxIndex { get; }

        public Timeline(Epic[] epics)
        {
            Start = epics.OrderBy(e => e.CalculatedStartDate).Select(e => e.CalculatedStartDate).FirstOrDefault();
            if (Start == default)
                Start = DateTimeOffset.UtcNow.Date;
            Start = Start.GetFirstOfMonth();

            End = epics.OrderByDescending(e => e.CalculatedDueDate).Select(e => e.CalculatedDueDate).FirstOrDefault();
            if (End == default)
                End = DateTimeOffset.UtcNow.Date;
            End = End.AddMonths(1).GetFirstOfMonth();

            Projects = LayoutProjects(epics);
            Epics = GetEpics().ToArray();
            Rows = Epics.Max(e => e.row) + 1;
            MaxIndex = Epics.Max(e => e.endIndex) + 1;
        }

        private ProjectLayout[] LayoutProjects(Epic[] epics)
        {
            return epics.GroupBy(e => e.Project).Select(g => new ProjectLayout(g.Key, LayoutEpics(g))).ToArray();
        }

        private IReadOnlyList<IReadOnlyList<Epic>> LayoutEpics(IEnumerable<Epic> epics)
        {
            var lanes = new List<LinkedList<Epic>>();

            bool Allocate(Epic e)
            {
                foreach (var lane in lanes)
                {
                    var node = lane.First;
                    bool overlaps = false;
                    while (node != null)
                    {
                        if ((overlaps = e.Overlaps(node.Value)) == true)
                            break;
                        if (node.Value.CalculatedStartDate >= e.CalculatedDueDate)
                        {
                            lane.AddBefore(node, e);
                            return true;
                        }

                        node = node.Next;
                    }

                    if (!overlaps)
                    {
                        lane.AddLast(e);
                        return true;
                    }
                }

                return false;
            }

            foreach (var epic in epics)
            {
                if (Allocate(epic))
                    continue;
                lanes.Add(new LinkedList<Epic>());
                lanes.Last().AddLast(epic);
            }

            return lanes.Select(l => l.ToArray()).ToArray();
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

        public IEnumerable<(DateTimeOffset day, int index)> GetMonths()
        {
            var day = Start;
            if (day.Day != 1)
                day = day.AddMonths(1).GetFirstOfMonth();
            while (day < End)
            {
                yield return GetDayWithIndex(day);
                day = day.AddMonths(1);
            }
        }

        public IEnumerable<(DateTimeOffset day, int index)> GetToday()
        {
            var today = DateTimeOffset.UtcNow.Date.AddDays(1);
            if (today >= Start && today <= End)
                yield return GetDayWithIndex(today);
        }

        private IEnumerable<(Epic epic, int startIndex, int endIndex, int row)> GetEpics()
        {
            int row = 1;
            foreach (var p in Projects)
            {
                foreach (var epicRow in p.Epics)
                {
                    foreach (var epic in epicRow)
                        yield return (epic, GetDayIndex(epic.CalculatedStartDate), GetDayIndex(epic.CalculatedDueDate), row);
                    ++row;
                }
            }
        }

        private (DateTimeOffset day, int index) GetDayWithIndex(in DateTimeOffset day) => (day, GetDayIndex(day));

        private int GetDayIndex(DateTimeOffset day)
        {
            return (int)(day - Start).TotalDays;
        }
    }
}
