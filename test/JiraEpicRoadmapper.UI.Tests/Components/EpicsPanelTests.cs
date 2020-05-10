using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Css.Dom;
using Bunit;
using Castle.Components.DictionaryAdapter;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Formatting;
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
                    x => x.Then_it_should_have_width_and_height(x.Timeline.TotalDays * LayoutSettings.DaySpan, x.Timeline.TotalRows * LayoutSettings.RowHeight))
                .RunAsync();
        }

        public class EpicsPanelFixture : ComponentFixture<EpicsPanel>
        {
            private readonly List<Epic> _epics = new List<Epic>();

            public Timeline Timeline => Component.Instance.Timeline;

            public void Given_a_epics_panel()
            {
                WithComponentParameter(ComponentParameter.CreateParameter(nameof(EpicsPanel.Epics), _epics));
            }

            public void Then_it_should_have_specified_timeline()
            {
                Component.Instance.Timeline.ShouldNotBeNull();
            }

            public void Then_it_should_have_width_and_height([Format("{0}px")]int width, [Format("{0}px")]int height)
            {
                var css = Component.Find("div.epics-panel").GetStyle();
                css.GetPropertyValue("width").ShouldBe($"{width}px");
                css.GetPropertyValue("height").ShouldBe($"{height}px");
            }
        }
    }
}