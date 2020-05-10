using System;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    public class TimelineTests
    {
        [Theory]
        [InlineData("2020-03-16", "2020-03-18", "2020-03-10", "2020-03-19", "2020-03-18", "2020-03-03", "2020-03-26", 23)]
        [InlineData("2020-03-16", "2020-03-18", "2020-03-10", "2020-03-19", "2020-03-08", "2020-03-01", "2020-03-26", 25)]
        [InlineData("2020-03-16", "2020-03-18", "2020-03-10", "2020-03-19", "2020-03-20", "2020-03-03", "2020-03-27", 24)]
        [InlineData("2020-03-19", null, null, "2020-03-10", "2020-03-18", "2020-03-03", "2020-03-26", 23)]
        [InlineData(null, null, null, null, "2020-03-18", "2020-03-11", "2020-03-25", 14)]
        public void It_should_set_Start_and_End_dates_to_widest_provided_and_buffer_of_7_days(
            string start1, string end1,
            string start2, string end2,
            string today,
            string expectedStart, string expectedEnd, int expectedTotalDays)
        {
            var timeline = new Timeline(new[]
            {
                new Epic{StartDate = ToDateTime(start1),DueDate = ToDateTime(end1)},
                new Epic{StartDate = ToDateTime(start2),DueDate = ToDateTime(end2)}
            }, ToDateTime(today));

            timeline.Start.ShouldBe(ToDateTime(expectedStart).Value);
            timeline.End.ShouldBe(ToDateTime(expectedEnd).Value);
            timeline.TotalDays.ShouldBe(expectedTotalDays);
        }

        [Fact]
        public void It_should_use_allow_building_timeline_for_no_epics()
        {
            var today = DateTimeOffset.Now.Date;
            var timeline = new Timeline(new Epic[] { }, today);
            timeline.Start.ShouldBe(today.AddDays(-7));
            timeline.End.ShouldBe(today.AddDays(7));
        }

        [Fact]
        public void It_should_set_today()
        {
            var today = DateTimeOffset.Now.Date;
            var someDay = today.AddDays(-5);

            new Timeline(new Epic[] { }, someDay).Today.ShouldBe(someDay);
            new Timeline(new Epic[] { }).Today.ShouldBe(today);
        }

        private static DateTimeOffset? ToDateTime(string date)
        {
            return DateTimeOffset.TryParse(date, out var result)
                ? (DateTimeOffset?)result
                : null;
        }
    }
}
