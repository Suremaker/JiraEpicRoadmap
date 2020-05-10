using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.UI.Models
{
    public class Timeline
    {
        public DateTimeOffset Today { get; }
        public DateTimeOffset Start { get; }
        public DateTimeOffset End { get; }
        public int TotalDays { get; }

        public Timeline(DateTimeOffset start, DateTimeOffset end, DateTimeOffset today)
        {
            if (start > today)
                throw new ArgumentOutOfRangeException(nameof(start), "Start cannot be greater than today");
            if (end < today)
                throw new ArgumentOutOfRangeException(nameof(end), "End cannot be less than today");

            Start = start;
            End = end;
            Today = today;
            TotalDays = GetDayIndex(End);
        }

        public static Timeline FromEpics(IReadOnlyList<Epic> epics, DateTimeOffset? today = null)
        {
            var todayDate = today ?? DateTimeOffset.Now.Date;
            var start = epics.Select(e => e.StartDate.GetValueOrDefault(e.DueDate.GetValueOrDefault(todayDate))).Append(todayDate).Min().AddDays(-7);
            var end = epics.Select(e => e.DueDate.GetValueOrDefault(e.StartDate.GetValueOrDefault(todayDate))).Append(todayDate).Max().AddDays(7);

            return new Timeline(start, end, todayDate);
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

        private int GetDayIndex(DateTimeOffset day) => (int)(day - Start).TotalDays;
        private (DateTimeOffset day, int index) GetDayWithIndex(in DateTimeOffset day) => (day, GetDayIndex(day));
    }
}
