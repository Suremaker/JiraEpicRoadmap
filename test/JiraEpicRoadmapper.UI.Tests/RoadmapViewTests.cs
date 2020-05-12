using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Pages;
using JiraEpicRoadmapper.UI.Repositories;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Shouldly;

namespace JiraEpicRoadmapper.UI.Tests
{
    public class RoadmapViewTests : FeatureFixture
    {
        [Scenario]
        public async Task Loading_epics()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_a_freshly_opened_page(),
                    x => x.Given_api_takes_a_while_to_load(),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_loading_bar_with_text("Loading..."),
                    x => x.Then_page_should_query_for_epics(),
                    x => x.Then_error_panel_should_not_be_visible(),
                    x => x.Then_epics_panel_should_not_be_visible())
                .RunAsync();
        }

        [Scenario]
        public async Task Display_loading_error()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_a_freshly_opened_page(),
                    x => x.Given_api_cannot_return_epics_due_to_reason("something"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_error_panel_with_text("Unable to load epics, here is panda instead: 🐼\n\nFailure reason:\nsomething"),
                    x => x.Then_loading_bar_should_not_be_visible(),
                    x => x.Then_epics_panel_should_not_be_visible())
                .RunAsync();
        }

        [Scenario]
        public async Task Successful_loading()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_a_freshly_opened_page(),
                    x => x.Given_api_successfully_returns_epics(),
                    x => x.When_I_render_it(),
                    x => x.Then_loading_bar_should_not_be_visible(),
                    x => x.Then_error_panel_should_not_be_visible(),
                    x => x.Then_epics_panel_should_be_visible_with_associated_epics(),
                    x => x.Then_view_should_request_scroll_to_today_minus_1_day()
                    )
                .RunAsync();
        }

        public class RoadmapViewFixture : ComponentFixture<RoadmapView>
        {
            private readonly Mock<IEpicsRepository> _repository = new Mock<IEpicsRepository>();
            private readonly Mock<IJSRuntime> _jsRuntime = new Mock<IJSRuntime>();

            public void Given_a_freshly_opened_page()
            {
                Services.AddSingleton<IStatusVisualizer>(new StatusVisualizer());
                Services.AddSingleton<IEpicCardPainter>(new EpicCardPainter());
                Services.AddSingleton<IEpicsRepository>(_repository.Object);
                Services.AddSingleton<IJSRuntime>(_jsRuntime.Object);
            }

            public void Then_I_should_see_loading_bar_with_text(string text)
            {
                Component.FindComponents<LoadingBar>().Single(x => x.Instance.Id == "mainLoadingBar").Instance.Label.ShouldBe(text);
            }

            public void Then_page_should_query_for_epics()
            {
                _repository.Verify(r => r.FetchEpics());
            }

            public void Given_api_cannot_return_epics_due_to_reason(string reason)
            {
                _repository.Setup(r => r.FetchEpics()).ThrowsAsync(new Exception(reason));
            }

            public void Given_api_successfully_returns_epics()
            {
                _repository.Setup(r => r.FetchEpics()).ReturnsAsync(new[] { new Epic { Id = "foo" } });
            }

            public void Given_api_takes_a_while_to_load()
            {
                _repository.Setup(r => r.FetchEpics()).Returns(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    return null;
                });
            }

            public void Then_I_should_see_error_panel_with_text(string text)
            {
                Component.FindComponent<ErrorPanel>().Instance.Errors.ShouldBe(text);
            }

            public void Then_loading_bar_should_not_be_visible()
            {
                Component.FindComponents<LoadingBar>().Where(x => x.Instance.Id == "mainLoadingBar").ShouldBeEmpty();
            }

            public void Then_error_panel_should_not_be_visible()
            {
                Component.FindComponents<ErrorPanel>().ShouldBeEmpty();
            }

            public void Then_epics_panel_should_be_visible_with_associated_epics()
            {
                Component.FindComponent<EpicsPanel>().Instance.Epics.ShouldNotBeEmpty();
            }

            public void Then_epics_panel_should_not_be_visible()
            {
                Component.FindComponents<EpicsPanel>().ShouldBeEmpty();
            }

            public void Then_view_should_request_scroll_to_today_minus_1_day()
            {
                var todayIndex = Component.FindComponent<EpicsPanel>().Instance.Roadmap.Timeline.Today.Index;
                _jsRuntime.Verify(x => x.InvokeAsync<object>("scroll", CancellationToken.None, new object[]
                {
                    (todayIndex - 1) * LayoutSettings.DaySpan,
                    0
                }));
            }
    }
}
}
