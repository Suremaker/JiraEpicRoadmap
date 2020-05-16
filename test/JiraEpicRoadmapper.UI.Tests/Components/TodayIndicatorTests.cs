using System;
using System.Threading.Tasks;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Shared.Components;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class TodayIndicatorTests : FeatureFixture
    {
        [Scenario]
        public async Task Today_indicator_with_date_set()
        {
            await Runner
                .WithContext<TodayIndicatorFixture>()
                .AddSteps(
                    x => x.Given_a_today_indicator(),
                    x => x.Given_it_has_parameter_value(nameof(DayIndicator.Day), new IndexedDay(DateTimeOffset.Parse("2020-05-03"), 5)),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content($"<div class=\"today-indicator\" style=\"left:{5 * LayoutSettings.DaySpan}px\"><br/>03/05 (today)</div>"))
                .RunAsync();
        }

        public class TodayIndicatorFixture : ComponentFixture<TodayIndicator>
        {
            public void Given_a_today_indicator()
            {
            }
        }
    }
}