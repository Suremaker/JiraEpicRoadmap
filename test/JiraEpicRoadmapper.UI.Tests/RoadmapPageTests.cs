using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Pages;
using JiraEpicRoadmapper.UI.Repositories;
using JiraEpicRoadmapper.UI.Services;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Shared.Components;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Shouldly;

namespace JiraEpicRoadmapper.UI.Tests
{
    public class RoadmapPageTests : FeatureFixture
    {
        [Scenario]
        public async Task Loading_epics()
        {
            await Runner
                .WithContext<RoadmapPageFixture>()
                .AddSteps(
                    x => x.Given_a_freshly_opened_page(),
                    x => x.Given_api_takes_a_while_to_load(),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_loading_bar_with_text("Loading..."),
                    x => x.Then_page_should_query_for_epics(),
                    x => x.Then_error_panel_should_not_be_visible(),
                    x => x.Then_roadmap_view_should_not_be_visible())
                .RunAsync();
        }

        [Scenario]
        public async Task Display_loading_error()
        {
            await Runner
                .WithContext<RoadmapPageFixture>()
                .AddSteps(
                    x => x.Given_a_freshly_opened_page(),
                    x => x.Given_api_cannot_return_epics_due_to_reason("something"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_error_panel_with_text("Unable to load epics, here is panda instead: 🐼\n\nFailure reason:\nsomething"),
                    x => x.Then_loading_bar_should_not_be_visible(),
                    x => x.Then_roadmap_view_should_not_be_visible())
                .RunAsync();
        }

        [Scenario]
        public async Task Successful_loading()
        {
            await Runner
                .WithContext<RoadmapPageFixture>()
                .AddSteps(
                    x => x.Given_a_freshly_opened_page(),
                    x => x.Given_api_successfully_returns_epics(),
                    x => x.When_I_render_it(),
                    x => x.Then_loading_bar_should_not_be_visible(),
                    x => x.Then_error_panel_should_not_be_visible(),
                    x => x.Then_roadmap_view_should_be_visible_with_associated_epics(),
                    x => x.Then_control_panel_should_be_visible(),
                    x => x.Then_view_should_request_scroll_to_today_minus_1_day(),
                    x => x.Then_every_epic_stats_should_be_requested_and_updated()
                    )
                .RunAsync();
        }

        [Scenario]
        public async Task Updating_epic_selection()
        {
            await Runner
                .WithContext<RoadmapPageFixture>()
                .AddSteps(
                    x => x.Given_a_freshly_opened_page(),
                    x => x.Given_api_successfully_returns_epics(),
                    x => x.When_I_render_it())
                .AddAsyncSteps(
                    x => x.When_I_select_epic_card_on_epic_view())
                .AddSteps(
                    x => x.Then_the_selected_epic_should_propagate_to_roadmap_view(),
                    x => x.Then_the_selected_epic_should_propagate_to_control_panel()
                )
                .RunAsync();
        }

        [Scenario]
        public async Task Refreshing_roadmap_on_options_change()
        {
            await Runner
                .WithContext<RoadmapPageFixture>()
                .AddSteps(
                    x => x.Given_a_freshly_opened_page(),
                    x => x.Given_api_successfully_returns_epics(),
                    x => x.When_I_render_it())
                .AddAsyncSteps(
                    x => x.When_I_change_the_view_options())
                .AddSteps(
                    x => x.Then_roadmap_layout_should_get_updated()
                )
                .RunAsync();
        }

        //TODO: add test for updating epic

        public class RoadmapPageFixture : ComponentFixture<RoadmapPage>
        {
            private readonly Mock<IEpicsRepository> _repository = new Mock<IEpicsRepository>();
            private readonly Mock<IJSRuntime> _jsRuntime = new Mock<IJSRuntime>();
            private readonly List<Epic> _epics = new List<Epic>();
            private State<EpicCard> _selectedCard;
            private readonly IViewOptions _viewOptions = new TestableViewOptions();
            private readonly Mock<ILayoutDesigner> _layoutDesigner = new Mock<ILayoutDesigner>();
            private EpicCard SelectedCard
            {
                get => _selectedCard;
                set => _selectedCard = new State<EpicCard>(value);
            }

            public RoadmapPageFixture()
            {
                _repository
                    .Setup(r => r.FetchEpicStats(It.IsAny<string>()))
                    .ReturnsAsync(() => new EpicStats { NotStarted = 1 });

                _layoutDesigner.Setup(x => x.Layout(It.IsAny<IEnumerable<EpicMetadata>>()))
                    .Returns((IEnumerable<EpicMetadata> e) => new[] { e });
            }

            public void Given_a_freshly_opened_page()
            {
                Services.AddSingleton<IStatusVisualizer>(new StatusVisualizer());
                Services.AddSingleton<IEpicCardPainter>(new EpicCardPainter());
                Services.AddSingleton<ILayoutDesigner>(_layoutDesigner.Object);
                Services.AddSingleton<IViewOptions>(_viewOptions);
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
                _epics.Add(new Epic { Id = "foo", Key = "PR-foo", StartDate = DateTimeOffset.Now });
                _epics.Add(new Epic { Id = "bar", Key = "PR-bar", StartDate = DateTimeOffset.Now });
                _repository.Setup(r => r.FetchEpics()).ReturnsAsync(_epics.ToArray());
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

            public void Then_roadmap_view_should_be_visible_with_associated_epics()
            {
                Component.FindComponent<RoadmapView>().Instance.Roadmap.ShouldNotBeNull();
            }

            public void Then_control_panel_should_be_visible()
            {
                Component.FindComponents<ControlPanel>().ShouldNotBeEmpty();
            }

            public void Then_roadmap_view_should_not_be_visible()
            {
                Component.FindComponents<RoadmapView>().ShouldBeEmpty();
            }

            public void Then_view_should_request_scroll_to_today_minus_1_day()
            {
                var todayIndex = Component.FindComponent<RoadmapView>().Instance.Roadmap.Timeline.Today.Index;
                _jsRuntime.Verify(x => x.InvokeAsync<object>("scroll", CancellationToken.None, new object[]
                {
                    (todayIndex - 1) * LayoutSettings.DaySpan,
                    0
                }));
            }

            public void Then_every_epic_stats_should_be_requested_and_updated()
            {
                foreach (var epic in _epics)
                {
                    _repository.Verify(r => r.FetchEpicStats(epic.Key));
                    Component.FindComponent<RoadmapView>().Instance.Roadmap.Map.TryGetById(epic.Id).Stats.ShouldNotBeNull();
                }
            }

            public async Task When_I_select_epic_card_on_epic_view()
            {
                var view = Component.FindComponent<RoadmapView>().Instance;
                SelectedCard = view.Roadmap.EpicCards.First();
                await Renderer.Dispatcher.InvokeAsync(() => view.OnEpicSelect.InvokeAsync(SelectedCard));
            }

            public void Then_the_selected_epic_should_propagate_to_roadmap_view()
            {
                Component.FindComponent<RoadmapView>().Instance.SelectedEpic.ShouldBeSameAs(SelectedCard);
            }

            public void Then_the_selected_epic_should_propagate_to_control_panel()
            {
                Component.FindComponent<ControlPanel>().Instance.SelectedEpic.ShouldBeSameAs(SelectedCard);
            }

            public async Task When_I_change_the_view_options()
            {
                await Renderer.Dispatcher.InvokeAsync(() => _viewOptions.ToggleClosed());
            }

            public void Then_roadmap_layout_should_get_updated()
            {
                _layoutDesigner.Verify(x => x.Layout(It.IsAny<IEnumerable<EpicMetadata>>()), Times.AtLeast(2));
            }
        }
    }
}
