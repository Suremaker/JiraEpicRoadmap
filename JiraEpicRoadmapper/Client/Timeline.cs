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
        public int FirstRowIndex { get; } = 1;

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
            var remaining = epics.ToDictionary(e => e.Id, e => new EpicTree(e));
            foreach (var e in remaining.Values)
            {
                e.CalculateDepth(remaining);
                e.ProcessReferences(remaining);
            }

            var lanes = new List<LinkedList<Epic>>();

            while (remaining.Count > 0)
            {
                var epic = remaining.Values.OrderBy(r => r.InboundRefs).ThenBy(r => r.Epic.CalculatedStartDate).ThenByDescending(r => r.Depth).First();
                Allocate(epic.Epic);
            }

            void Allocate(Epic e)
            {
                if (!remaining.Remove(e.Id))
                    return;

                bool added = false;
                foreach (var lane in lanes)
                {
                    if (added) break;
                    var node = lane.First;
                    bool overlaps = false;
                    while (node != null)
                    {
                        if ((overlaps = e.Overlaps(node.Value)) == true)
                            break;
                        if (node.Value.CalculatedStartDate >= e.CalculatedDueDate)
                        {
                            overlaps = true;
                            break;
                        }

                        node = node.Next;
                    }

                    if (!overlaps && !added && (e.DependsOn(lane.Last.Value) || !lane.Last.Value.Links.Any()))
                    {
                        lane.AddLast(e);
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    lanes.Add(new LinkedList<Epic>());
                    lanes.Last().AddLast(e);
                }
                var subEpics = EpicTree.GetSubEpics(e, remaining).OrderByDescending(r => r.Depth).ToArray();
                foreach (var se in subEpics)
                    Allocate(se.Epic);
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

        public IEnumerable<(string project, int row)> GetProjectRows()
        {
            int row = FirstRowIndex;
            foreach (var p in Projects)
            {
                yield return (p.Name, row);
                row += p.Epics.Count + 1;
            }
        }

        public IEnumerable<(DateTimeOffset day, int index)> GetToday()
        {
            var today = DateTimeOffset.UtcNow.Date;
            if (today >= Start && today <= End)
                yield return GetDayWithIndex(today);
        }

        private IEnumerable<(Epic epic, int startIndex, int endIndex, int row)> GetEpics()
        {
            int row = FirstRowIndex;
            foreach (var p in Projects)
            {
                ++row;
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

    class EpicTree
    {
        public Epic Epic { get; }
        public int Depth { get; private set; } = -1;
        public int InboundRefs { get; private set; } = 0;
        public EpicTree(Epic e) { Epic = e; }
        public void IncrementInboundRef() => InboundRefs++;
        public EpicTree CalculateDepth(IReadOnlyDictionary<string, EpicTree> epics)
        {
            if (Depth < 0)
            {
                Depth = GetSubEpics(Epic, epics)
                .Select(e => (int?)e.CalculateDepth(epics).Depth)
                .OrderByDescending(x => x)
                .FirstOrDefault()
                .GetValueOrDefault(-1) + 1;
            }
            return this;
        }

        public static IEnumerable<EpicTree> GetSubEpics(Epic e, IReadOnlyDictionary<string, EpicTree> epics)
        {
            return e.Links
                .Select(l => epics.TryGetValue(l.OutwardId, out var e) ? e : null)
                .Where(e => e != null);
        }

        public void ProcessReferences(Dictionary<string, EpicTree> epics)
        {
            foreach (var e in GetSubEpics(Epic, epics))
                e.IncrementInboundRef();
        }
    }
}
