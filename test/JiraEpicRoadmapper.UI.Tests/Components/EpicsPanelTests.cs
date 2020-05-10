using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Css.Dom;
using Bunit;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Formatting;
using LightBDD.Framework.Parameters;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Shouldly;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class EpicsPanelTests : FeatureFixture
    {
        [Scenario]
        public async Task Panel_should_resize_to_fit_the_timeline()
        {
            await Runner
                .WithContext<EpicsPanelFixture>()
                .AddSteps(
                    x => x.Given_a_epics_panel(),
                    x => x.When_I_render_it(),
                    x => x.Then_it_should_have_specified_timeline(),
                    x => x.Then_it_should_have_width_and_height(x.EpicsRoadmap.TotalDays * LayoutSettings.DaySpan, x.EpicsRoadmap.TotalRows * LayoutSettings.RowHeight))
                .RunAsync();
        }

        [Scenario]
        public async Task Panel_should_display_the_timeline()
        {
            await Runner
                .WithContext<EpicsPanelFixture>()
                .AddSteps(
                    x => x.Given_a_epics_panel(),
                    x => x.Given_a_epics_covering_time_from_to("2020-03-04", "2020-03-25"),
                    x => x.Given_today_is_DATE("2020-03-15"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_the_day_indicators(x.EpicsRoadmap.Timeline.GetMondays()
                        .ToVerifiableDataTable(t => t.WithKey(c => c.Date).WithInferredColumns())),
                    x => x.Then_I_should_see_the_today_indicator_with_valid_date()
                )
                .RunAsync();
        }

        public class EpicsPanelFixture : ComponentFixture<EpicsPanel>
        {
            private readonly List<Epic> _epics = new List<Epic>();
            private DateTimeOffset _today;

            public EpicsRoadmap EpicsRoadmap => Component.Instance.Roadmap;

            public void Given_a_epics_panel()
            {
                WithComponentParameter(ComponentParameter.CreateParameter(nameof(EpicsPanel.Epics), _epics));
            }

            public void Given_today_is_DATE(string date)
            {
                _today = DateTimeOffset.Parse(date);
                WithComponentParameter(ComponentParameter.CreateParameter(nameof(EpicsPanel.Today), _today));
            }

            public void Then_it_should_have_specified_timeline()
            {
                Component.Instance.Roadmap.ShouldNotBeNull();
            }

            public void Then_it_should_have_width_and_height([Format("{0}px")]int width, [Format("{0}px")]int height)
            {
                var css = Component.Find("div.epics-panel").GetStyle();
                css.GetPropertyValue("width").ShouldBe($"{width}px");
                css.GetPropertyValue("height").ShouldBe($"{height}px");
            }

            public void Given_a_epics_covering_time_from_to(string from, string to)
            {
                _epics.Add(new Epic { StartDate = DateTimeOffset.Parse(from), DueDate = DateTimeOffset.Parse(to), Id = "foo" });
            }

            public void Then_I_should_see_the_day_indicators(VerifiableDataTable<IndexedDay> indicators)
            {
                indicators.SetActual(Component.FindComponents<DayIndicator>().Select(x => x.Instance.Day));
            }

            public void Then_I_should_see_the_today_indicator_with_valid_date()
            {
                Component.FindComponent<TodayIndicator>().Instance.Day.Date.ShouldBe(_today);
            }
        }
    }
}