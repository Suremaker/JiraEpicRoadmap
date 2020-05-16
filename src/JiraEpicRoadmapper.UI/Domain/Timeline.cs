using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;

namespace JiraEpicRoadmapper.UI.Domain
{
    public class Timeline
    {
        public const int WeekDays = 7;
        public DateTimeOffset Start { get; }
        public DateTimeOffset End { get; }
        public int TotalDays { get; }
        public IndexedDay Today { get; }

        public Timeline(DateTimeOffset start, DateTimeOffset end, DateTimeOffset today)
        {
            if (start > today)
                throw new ArgumentOutOfRangeException(nameof(start), "Start cannot be greater than today");
            if (end < today)
                throw new ArgumentOutOfRangeException(nameof(end), "End cannot be less than today");

            Start = start;
            End = end;
            Today = GetDayWithIndex(today);
            TotalDays = GetDayIndex(End);
        }

        public static Timeline FromEpics(IReadOnlyList<Epic> epics, DateTimeOffset? today = null)
        {
            var todayDate = today ?? DateTime.UtcNow.Date;
            var start = epics.Select(e => e.StartDate.GetValueOrDefault(e.DueDate.GetValueOrDefault(todayDate))).Append(todayDate).Min().AddDays(-WeekDays);
            var end = epics.Select(e => e.DueDate.GetValueOrDefault(e.StartDate.GetValueOrDefault(todayDate))).Append(todayDate).Max().AddDays(WeekDays);

            return new Timeline(start, end, todayDate);
        }

        public IEnumerable<IndexedDay> GetMondays()
        {
            var day = Start;
            while (day.DayOfWeek != DayOfWeek.Monday)
                day = day.AddDays(1);
            while (day < End)
            {
                yield return GetDayWithIndex(day);
                day = day.AddDays(WeekDays);
            }
        }

        public IndexedDay GetDayWithIndex(in DateTimeOffset day) => new IndexedDay(day, GetDayIndex(day));
        private int GetDayIndex(DateTimeOffset day) => (int)(day - Start).TotalDays;
    }
}
