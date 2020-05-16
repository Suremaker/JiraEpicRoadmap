using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Css.Dom;
using Bunit;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Domain;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Repositories;
using JiraEpicRoadmapper.UI.Services;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Formatting;
using LightBDD.Framework.Parameters;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
                    x => x.Given_epics_covering_time_from_to("2020-03-04", "2020-03-25"),
                    x => x.Given_today_is_DATE("2020-03-15"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_the_day_indicators(x.EpicsRoadmap.Timeline.GetMondays()
                        .ToVerifiableDataTable(t => t.WithKey(c => c.Date).WithInferredColumns())),
                    x => x.Then_I_should_see_the_today_indicator_with_valid_date(),
                    x => x.Then_panel_should_request_scroll_to_today_position(x.EpicsRoadmap.Timeline.Today.Index * LayoutSettings.DaySpan)
                )
                .RunAsync();
        }

        [Scenario]
        public async Task Panel_should_display_project_names()
        {
            await Runner
                .WithContext<EpicsPanelFixture>()
                .AddSteps(
                    x => x.Given_a_epics_panel(),
                    x => x.Given_epics_covering_time_from_to_for_projects("2020-03-04", "2020-03-25", "Project X", "Project Y", "Project Z"),
                    x => x.Given_today_is_DATE("2020-03-15"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_the_project_headers(Table.ExpectData(
                        new ExpectedProjectHeader("Project X", 2, 5),
                        new ExpectedProjectHeader("Project Y", 4, 5),
                        new ExpectedProjectHeader("Project Z", 6, 5)))
                )
                .RunAsync();
        }

        [Scenario]
        public async Task Panel_should_display_epics()
        {
            await Runner
                .WithContext<EpicsPanelFixture>()
                .AddSteps(
                    x => x.Given_a_epics_panel(),
                    x => x.Given_epics(Table.For(
                        new Epic { Id = "EP-1", Project = "EP", Summary = "Hello world" },
                        new Epic { Id = "EP-2", Project = "EP", Summary = "Something done" },
                        new Epic { Id = "EP-3", Project = "EP", Summary = "Something to do" })),
                    x => x.Given_today_is_DATE("2020-03-15"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_the_specified_epics(x.Epics.ToVerifiableDataTable())
                )
                .RunAsync();
        }

        public class EpicsPanelFixture : ComponentFixture<EpicsPanel>
        {
            private readonly List<Epic> _epics = new List<Epic>();
            private readonly List<int> _scrollToTodayRequests = new List<int>();
            private DateTimeOffset _today;
            private Mock<IViewOptions> _viewOptions = new Mock<IViewOptions>();
            public IReadOnlyList<Epic> Epics => _epics;
            public EpicsRoadmap EpicsRoadmap => Component.Instance.Roadmap;

            public void Given_a_epics_panel()
            {
                _viewOptions.SetupGet(x => x.ShowUnplanned).Returns(true);
                _viewOptions.SetupGet(x => x.ShowClosed).Returns(true);

                Services.AddSingleton<IStatusVisualizer>(new StatusVisualizer());
                Services.AddSingleton<IEpicCardPainter>(new EpicCardPainter());
                Services.AddSingleton<IEpicsRepository>(Mock.Of<IEpicsRepository>());
                Services.AddSingleton<IViewOptions>(_viewOptions.Object);

                WithComponentParameter(ComponentParameter.CreateParameter(nameof(EpicsPanel.Epics), _epics));
                WithComponentParameter(EventCallback<int>(nameof(EpicsPanel.OnScrollToTodayRequest), (position) => _scrollToTodayRequests.Add(position)));
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
                Component.FindComponent<TodayIndicator>().Instance.Day.Date.ShouldBe(_today);
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

            public void Then_panel_should_request_scroll_to_today_position(Verifiable<int> position)
            {
                position.SetActual(() => _scrollToTodayRequests.Single());
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