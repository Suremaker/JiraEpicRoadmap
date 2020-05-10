using System;
using System.Collections.Generic;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.UI.Models
{
    public class EpicsTimeline
    {
        public Timeline Timeline { get; }
        public int TotalDays => Timeline.TotalDays;
        public int TotalRows { get; } = 1;

        public EpicsTimeline(IReadOnlyList<Epic> epics, DateTimeOffset? today = null)
        {
            Timeline = Timeline.FromEpics(epics, today);
        }
    }
}