using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Css.Dom;
using Bunit;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Domain;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Services;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Shared.Components;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework;
using LightBDD.Framework.Formatting;
using LightBDD.Framework.Parameters;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class RoadmapViewTests : FeatureFixture
    {
        [Scenario]
        public async Task View_should_resize_to_fit_the_timeline()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_a_roadmap_view(),
                    x => x.When_I_render_it(),
                    x => x.Then_it_should_have_specified_timeline(),
                    x => x.Then_it_should_have_width_and_height(x.Roadmap.TotalDays * LayoutSettings.DaySpan, x.Roadmap.TotalRows * LayoutSettings.RowHeight))
                .RunAsync();
        }

        [Scenario]
        public async Task View_should_display_the_timeline()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_epics_covering_time_from_to("2020-03-04", "2020-03-25"),
                    x => x.Given_today_is_DATE("2020-03-15"),
                    x => x.Given_a_roadmap_view(),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_the_day_indicators(x.Roadmap.Timeline.GetMondays()
                        .ToVerifiableDataTable(t => t.WithKey(c => c.Date).WithInferredColumns())),
                    x => x.Then_I_should_see_the_today_indicator_with_valid_date(),
                    x => x.Then_View_should_request_scroll_to_today_position(x.Roadmap.Timeline.Today.Index * LayoutSettings.DaySpan)
                )
                .RunAsync();
        }

        [Scenario]
        public async Task View_should_display_project_names()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_epics_covering_time_from_to_for_projects("2020-03-04", "2020-03-25", "Project X", "Project Y", "Project Z"),
                    x => x.Given_today_is_DATE("2020-03-15"),
                    x => x.Given_a_roadmap_view(),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_the_project_headers(Table.ExpectData(
                        new ExpectedProjectHeader("Project X", 2, 5),
                        new ExpectedProjectHeader("Project Y", 4, 5),
                        new ExpectedProjectHeader("Project Z", 6, 5)))
                )
                .RunAsync();
        }

        [Scenario]
        public async Task View_should_display_epics()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_epics(Table.For(
                        new Epic { Id = "EP-1", Project = "EP", Summary = "Hello world" },
                        new Epic { Id = "EP-2", Project = "EP", Summary = "Something done" },
                        new Epic { Id = "EP-3", Project = "EP", Summary = "Something to do" })),
                    x => x.Given_a_roadmap_view(),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_the_specified_epics(x.Epics.ToVerifiableDataTable())
                )
                .RunAsync();
        }

        [Scenario]
        public async Task View_should_display_selected_epic()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_epics(Table.For(
                        new Epic { Id = "EP-1", Project = "EP", Summary = "Hello world" },
                        new Epic { Id = "EP-2", Project = "EP", Summary = "Something done" })),
                    x => x.Given_a_roadmap_view(),
                    x => x.Given_the_epic_is_selected("EP-1"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_epic_rendered_as_SELECTED("EP-1", true),
                    x => x.Then_I_should_see_epic_rendered_as_SELECTED("EP-2", false)
                )
                .RunAsync();
        }

        [Scenario]
        public async Task Clicking_on_epic_card_should_propagate_out_while_clicking_on_view_itself_should_reset_the_selection()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_epics(Table.For(
                        new Epic { Id = "EP-1", Project = "EP", Summary = "Hello world" },
                        new Epic { Id = "EP-2", Project = "EP", Summary = "Something done" })),
                    x => x.Given_today_is_DATE("2020-03-15"),
                    x => x.Given_a_roadmap_view(),
                    x => x.When_I_render_it(),
                    x => x.When_I_click_on_the_epic_card("EP-1"),
                    x => x.Then_OnEpicSelect_event_should_get_raised_for_epic("EP-1"),
                    x => x.When_I_click_on_the_epic_card("EP-2"),
                    x => x.Then_OnEpicSelect_event_should_get_raised_for_epic("EP-2"),
                    x => x.When_I_click_on_the_view_itself(),
                    x => x.Then_OnEpicSelect_event_should_get_raised_for_epic(null))
                .RunAsync();
        }

        public class RoadmapViewFixture : ComponentFixture<RoadmapView>
        {
            private readonly Mock<IViewOptions> _viewOptions = new Mock<IViewOptions>();
            private readonly List<Epic> _epics = new List<Epic>();
            private readonly List<int> _scrollToTodayRequests = new List<int>();
            private DateTimeOffset? _today;
            private State<EpicsRoadmap> _roadmap;
            private State<EpicCard> _onEpicSelected;
            private EpicCard OnEpicSelected
            {
                get => _onEpicSelected;
                set => _onEpicSelected = new State<EpicCard>(value);
            }

            public IReadOnlyList<Epic> Epics => _epics;

            public EpicsRoadmap Roadmap
            {
                get => _roadmap;
                set => _roadmap = new State<EpicsRoadmap>(value);
            }

            public void Given_a_roadmap_view()
            {
                _viewOptions.SetupGet(x => x.ShowUnplanned).Returns(true);
                _viewOptions.SetupGet(x => x.ShowClosed).Returns(true);
                Roadmap = new EpicsRoadmap(_epics, _today);
                Roadmap.UpdateLayout(new LayoutDesigner(), _viewOptions.Object);

                Services.AddSingleton<IStatusVisualizer>(new StatusVisualizer());
                Services.AddSingleton<IEpicCardPainter>(new EpicCardPainter());

                WithComponentParameter(ComponentParameter.CreateParameter(nameof(RoadmapView.Roadmap), Roadmap));
                WithComponentParameter(EventCallback<int>(nameof(RoadmapView.OnScrollToTodayRequest), (position) => _scrollToTodayRequests.Add(position)));
                WithComponentParameter(EventCallback<EpicCard>(nameof(RoadmapView.OnEpicSelect), e => OnEpicSelected = e));
            }

            public void Given_today_is_DATE(string date)
            {
                _today = DateTimeOffset.Parse(date);
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

            public void Given_epics_covering_time_from_to(string from, string to)
            {
                _epics.Add(new Epic { StartDate = DateTimeOffset.Parse(from), DueDate = DateTimeOffset.Parse(to), Id = "foo" });
            }

            public void Given_epics_covering_time_from_to_for_projects(string from, string to, params string[] projects)
            {
                foreach (var project in projects)
                {
                    _epics.Add(new Epic { StartDate = DateTimeOffset.Parse(from), DueDate = DateTimeOffset.Parse(to), Id = $"{project}-1", Project = project });
                }
            }

            public void Then_I_should_see_the_day_indicators(VerifiableDataTable<IndexedDay> indicators)
            {
                indicators.SetActual(Component.FindComponents<DayIndicator>().Select(x => x.Instance.Day));
            }

            public void Then_I_should_see_the_today_indicator_with_valid_date()
            {
                Component.FindComponent<TodayIndicator>().Instance.Day.Date.ShouldBe(_today.GetValueOrDefault());
            }

            public void Then_I_should_see_the_project_headers(VerifiableDataTable<ExpectedProjectHeader> header)
            {
                header.SetActual(Component.FindComponents<ProjectHeader>().Select(h =>
                    new ExpectedProjectHeader(h.Instance.Name, h.Instance.RowIndex, h.Instance.DayIndex)));
            }

            public void Given_epics(InputTable<Epic> epics)
            {
                _epics.AddRange(epics);
            }

            public void Then_I_should_see_the_specified_epics(VerifiableDataTable<Epic> epics)
            {
                var actual = Component
                    .FindComponents<EpicCardView>()
                    .Select(v => v.Instance.Card.Meta.Epic)
                    .ToDictionary(x => x.Id);
                epics.SetActual(input => actual.TryGetValue(input.Id, out var r) ? r : null);
            }

            public void Then_View_should_request_scroll_to_today_position(Verifiable<int> position)
            {
                position.SetActual(() => _scrollToTodayRequests.Single());
            }

            public void When_I_click_on_the_epic_card(string card)
            {
                Component.FindComponents<EpicCardView>()
                    .Single(v => v.Instance.Card.Meta.Epic.Id == card)
                    .Find("div.epic-card")
                    .Click();
            }

            public void Then_OnEpicSelect_event_should_get_raised_for_epic(string epic)
            {
                if (epic == null)
                    OnEpicSelected.ShouldBe(null);
                else
                {
                    OnEpicSelected.ShouldNotBeNull();
                    OnEpicSelected.Meta.Epic.Id.ShouldBe(epic);
                }
            }

            public void When_I_click_on_the_view_itself()
            {
                Component.Find("div.epics-panel").Click();
            }

            public void Given_the_epic_is_selected(string epic)
            {
                WithComponentParameter(ComponentParameter.CreateParameter(nameof(RoadmapView.SelectedEpic), Roadmap.EpicCards.Single(c => c.Meta.Epic.Id == epic)));
            }

            public void Then_I_should_see_epic_rendered_as_SELECTED(string epic, [FormatBoolean("selected", "not selected")]bool selected)
            {
                Component.FindComponents<EpicCardView>()
                    .Single(v => v.Instance.Card.Meta.Epic.Id == epic)
                    .Instance.Selected.ShouldBe(selected);
            }
        }

        public class ExpectedProjectHeader
        {
            public ExpectedProjectHeader(string name, int rowIndex, int dayIndex)
            {
                Name = name;
                RowIndex = rowIndex;
                DayIndex = dayIndex;
            }

            public string Name { get; }
            public int RowIndex { get; }
            public int DayIndex { get; }
        }
    }
}