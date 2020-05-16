using System;
using System.Threading.Tasks;
using AngleSharp.Css.Dom;
using Bunit;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Repositories;
using JiraEpicRoadmapper.UI.Services;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework;
using LightBDD.Framework.Formatting;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class EpicCardViewTests : FeatureFixture
    {
        [Scenario]
        public async Task Epic_card_content()
        {
            await Runner
                .WithContext<EpicCardViewFixture>()
                .AddSteps(
                    x => x.Given_a_epic_card_view(),
                    x => x.Given_epic_has_summary("Hello world"),
                    x => x.Given_epic_has_status_category("in progress"),
                    x => x.Given_epic_repository_has_available_statistics_for_the_epic(),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_card_of_width_height_located_at_x_y(
                        (x.Block.EndIndex - x.Block.StartIndex) * LayoutSettings.DaySpan - 2 * LayoutSettings.CellMargin,
                        LayoutSettings.CardHeight,
                        x.Block.StartIndex * LayoutSettings.DaySpan + LayoutSettings.CellMargin,
                        x.Block.RowIndex * LayoutSettings.RowHeight + LayoutSettings.RowMargin),
                    x => x.Then_I_should_see_card_summary("Hello world"),
                    x => x.Then_I_should_see_card_status("⚙️"),
                    x => x.Then_I_should_see_card_details_progress_bar_with_loaded_stats())
                .RunAsync();
        }

        public class EpicCardViewFixture : ComponentFixture<EpicCardView>
        {
            private readonly Mock<IEpicsRepository> _repository = new Mock<IEpicsRepository>();
            public EpicCard Block { get; } = new EpicCard(new EpicMetadata(new Epic { Key = "FOO" }, new IndexedDay(DateTimeOffset.MinValue, 3), new IndexedDay(DateTimeOffset.MinValue, 5)), 1);
            public EpicMetadata Meta => Block.Meta;
            public Epic Epic => Meta.Epic;
            private State<EpicStats> _stats;
            public EpicStats Stats
            {
                get => _stats;
                set => _stats = new State<EpicStats>(value);
            }

            public void Given_a_epic_card_view()
            {
                Services.AddSingleton<IStatusVisualizer>(new StatusVisualizer());
                Services.AddSingleton<IEpicCardPainter>(new EpicCardPainter());
                Services.AddSingleton<IEpicsRepository>(_repository.Object);
                WithComponentParameter(ComponentParameter.CreateParameter(nameof(EpicCardView.Card), Block));
            }

            public void Given_epic_has_summary(string summary)
            {
                Epic.Summary = summary;
            }

            public void Given_epic_has_status_category(string category)
            {
                Epic.StatusCategory = category;
            }

            public void Then_I_should_see_card_of_width_height_located_at_x_y([Format("{0}px")]int width, [Format("{0}px")]int height, [Format("{0}px")]int x, [Format("{0}px")]int y)
            {
                var css = Component.Find("div.epic-card").GetStyle();
                css.GetPropertyValue("width").ShouldBe($"{width}px");
                css.GetPropertyValue("height").ShouldBe($"{height}px");
                css.GetPropertyValue("left").ShouldBe($"{x}px");
                css.GetPropertyValue("top").ShouldBe($"{y}px");
            }

            public void Then_I_should_see_card_summary(string summary)
            {
                Component.Find("div.summary").TextContent.ShouldBe(summary);
            }

            public void Then_I_should_see_card_status(string status)
            {
                Component.Find("div.status").TextContent.Trim().ShouldBe(status);
            }

            public void Then_I_should_see_card_details_progress_bar_with_loaded_stats()
            {
                Component.FindComponent<EpicProgressBar>().Instance.Stats.ShouldBeSameAs(Stats);
            }

            public void Given_epic_repository_has_available_statistics_for_the_epic()
            {
                _repository.Setup(r => r.FetchEpicStats(Epic.Key)).ReturnsAsync(Stats = new EpicStats { Done = 1, InProgress = 2, NotStarted = 3 });
            }
        }
    }
}