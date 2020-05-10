using System;
using System.Linq;
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
        public void FromEpics_should_set_Start_and_End_dates_to_widest_provided_and_buffer_of_7_days(
            string start1, string end1,
            string start2, string end2,
            string today,
            string expectedStart, string expectedEnd, int expectedTotalDays)
        {
            var timeline = Timeline.FromEpics(new[]
            {
                new Epic{StartDate = ToNullableDateTime(start1),DueDate = ToNullableDateTime(end1)},
                new Epic{StartDate = ToNullableDateTime(start2),DueDate = ToNullableDateTime(end2)}
            }, ToNullableDateTime(today));

            timeline.Start.ShouldBe(ToDateTime(expectedStart));
            timeline.End.ShouldBe(ToDateTime(expectedEnd));
            timeline.TotalDays.ShouldBe(expectedTotalDays);
        }

        [Fact]
        public void FromEpics_should_use_allow_building_timeline_for_no_epics()
        {
            var today = DateTimeOffset.Now.Date;
            var timeline = Timeline.FromEpics(new Epic[] { }, today);
            timeline.Start.ShouldBe(today.AddDays(-7));
            timeline.End.ShouldBe(today.AddDays(7));
        }

        [Fact]
        public void FromEpics_should_set_today()
        {
            var today = DateTimeOffset.Now.Date;
            var someDay = today.AddDays(-5);

            Timeline.FromEpics(new Epic[] { }, someDay).Today.ShouldBe(someDay);
            Timeline.FromEpics(new Epic[] { }).Today.ShouldBe(today);
        }

        [Fact]
        public void It_should_not_allow_today_parameter_being_outside_of_start_end_range()
        {
            var date = DateTimeOffset.Now.Date;
            Assert.Throws<ArgumentOutOfRangeException>(() => new Timeline(date.AddDays(-1), date.AddDays(+1), date.AddDays(-2))).Message.ShouldContain("Start cannot be greater than today");
            Assert.Throws<ArgumentOutOfRangeException>(() => new Timeline(date.AddDays(-1), date.AddDays(+1), date.AddDays(+2))).Message.ShouldContain("End cannot be less than today");
        }

        [Theory]
        [InlineData("2020-05-04", "2020-05-25", "2020-05-04|0", "2020-05-11|7", "2020-05-18|14")]
        [InlineData("2020-05-03", "2020-05-26", "2020-05-04|1", "2020-05-11|8", "2020-05-18|15", "2020-05-25|22")]
        public void GetMondays_returns_all_Mondays_with_index_within_the_Start_End_range(string start, string end, params string[] expected)
        {
            var startDate = ToDateTime(start);
            var endDate = ToDateTime(end);
            var timeline = new Timeline(startDate, endDate, endDate);
            timeline.GetMondays().Select(x => $"{x.day:yyyy-MM-dd}|{x.index}").ShouldBe(expected);
        }

        private static DateTimeOffset? ToNullableDateTime(string date)
        {
            return DateTimeOffset.TryParse(date, out var result)
                ? (DateTimeOffset?)result
                : null;
        }

        private static DateTimeOffset ToDateTime(string date) => DateTimeOffset.Parse(date);
    }
}
