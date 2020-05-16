using System;

namespace JiraEpicRoadmapper.UI.Models
{
    public readonly struct IndexedDay
    {
        public IndexedDay(DateTime date, int index)
        {
            Date = date;
            Index = index;
        }

        public DateTime Date { get; }
        public int Index { get; }

        public override string ToString() => $"{Index}:{Date:yyyy-MM-dd}";
    }
}