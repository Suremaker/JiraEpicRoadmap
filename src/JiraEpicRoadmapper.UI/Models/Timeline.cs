using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.UI.Models
{
    public class Timeline
    {
        public DateTimeOffset Today { get; }
        public DateTimeOffset Start { get; }
        public DateTimeOffset End { get; }
        public int TotalDays { get; }
        public int TotalRows { get; } = 1;

        public Timeline(IReadOnlyList<Epic> epics, DateTimeOffset? today = null)
        {
            Today = today ?? DateTimeOffset.Now.Date;
            Start = epics.Select(e => e.StartDate.GetValueOrDefault(e.DueDate.GetValueOrDefault(Today))).Append(Today).Min().AddDays(-7);
            End = epics.Select(e => e.DueDate.GetValueOrDefault(e.StartDate.GetValueOrDefault(Today))).Append(Today).Max().AddDays(7);

            TotalDays = GetDayIndex(End);
        }

        private int GetDayIndex(DateTimeOffset day) => (int)(day - Start).TotalDays;
    }
}
