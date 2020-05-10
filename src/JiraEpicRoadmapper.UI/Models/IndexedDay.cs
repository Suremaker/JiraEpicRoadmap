using System;
using System.IO.MemoryMappedFiles;

namespace JiraEpicRoadmapper.UI.Models
{
    public readonly struct IndexedDay
    {
        public IndexedDay(DateTimeOffset date, int index)
        {
            Date = date;
            Index = index;
        }

        public DateTimeOffset Date { get; }
        public int Index { get; }

        public override string ToString() => $"{Index}:{Date:yyyy-MM-dd}";
    }
}