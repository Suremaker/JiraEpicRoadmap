using System;
using System.Threading.Tasks;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Shared.Components;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class DayIndicatorTests : FeatureFixture
    {
        [Scenario]
        public async Task Day_indicator_with_date_set()
        {
            await Runner
                .WithContext<DayIndicatorFixture>()
                .AddSteps(
                    x => x.Given_a_day_indicator(),
                    x => x.Given_it_has_parameter_value(nameof(DayIndicator.Day), new IndexedDay(DateTime.Parse("2020-05-03"), 5)),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content($"<div class=\"day-indicator\" style=\"left:{5 * LayoutSettings.DaySpan}px\">03/05</div>"))
                .RunAsync();
        }

        public class DayIndicatorFixture : ComponentFixture<DayIndicator>
        {
            public void Given_a_day_indicator()
            {
            }
        }
    }
}