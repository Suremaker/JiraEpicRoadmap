using System;
using System.Threading.Tasks;
using AngleSharp.Css.Dom;
using Bunit;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Services;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Shared.Components;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework;
using LightBDD.Framework.Formatting;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Microsoft.Extensions.DependencyInjection;
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
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_card_of_width_height_located_at_x_y(
                        (x.Card.EndIndex - x.Card.StartIndex + 1) * LayoutSettings.DaySpan - 2 * LayoutSettings.CellMargin,
                        LayoutSettings.CardHeight,
                        x.Card.StartIndex * LayoutSettings.DaySpan + LayoutSettings.CellMargin,
                        x.Card.RowIndex * LayoutSettings.RowHeight + LayoutSettings.RowMargin),
                    x => x.Then_I_should_see_card_summary("Hello world"),
                    x => x.Then_I_should_see_card_status("⚙️"),
                    x => x.Then_I_should_see_card_details_progress_bar_with_no_stats())
                .AddAsyncSteps(
                    x => x.When_epic_stats_becomes_available())
                .AddSteps(
                    x => x.Then_I_should_see_card_details_progress_bar_with_loaded_stats())
                .RunAsync();
        }

        [Scenario]
        public async Task Selecting_card()
        {
            await Runner
                .WithContext<EpicCardViewFixture>()
                .AddSteps(
                    x => x.Given_a_epic_card_view(),
                    x => x.When_I_render_it(),
                    x => x.When_I_click_on_the_card(),
                    x => x.Then_OnCardSelect_event_should_get_raised())
                .RunAsync();
        }

        [Scenario]
        public async Task Selected_card()
        {
            await Runner
                .WithContext<EpicCardViewFixture>()
                .AddSteps(
                    x => x.Given_a_epic_card_view(),
                    x => x.Given_it_has_parameter_value(nameof(EpicCardView.Selected), true),
                    x => x.When_I_render_it(),
                    x => x.When_I_should_see_the_selected_status())
                .RunAsync();
        }

        public class EpicCardViewFixture : ComponentFixture<EpicCardView>
        {
            public EpicCard Card { get; } = new EpicCard(new EpicMetadata(new Epic { Key = "FOO" }, new IndexedDay(DateTime.MinValue, 3), new IndexedDay(DateTime.MinValue, 5)), 1);
            public EpicMetadata Meta => Card.Meta;
            public Epic Epic => Meta.Epic;
            private State<EpicStats> _stats;
            public EpicStats Stats
            {
                get => _stats;
                set => _stats = new State<EpicStats>(value);
            }
            private State<EpicCard> _cardClicked;
            public EpicCard CardClicked
            {
                get => _cardClicked;
                set => _cardClicked = new State<EpicCard>(value);
            }

            public void Given_a_epic_card_view()
            {
                Services.AddSingleton<IStatusVisualizer>(new StatusVisualizer());
                Services.AddSingleton<IEpicCardPainter>(new EpicCardPainter());
                WithComponentParameter(ComponentParameter.CreateParameter(nameof(EpicCardView.Card), Card));
                WithComponentParameter(EventCallback<EpicCard>(nameof(EpicCardView.OnCardSelect), c => CardClicked = c));
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

            public void Then_I_should_see_card_details_progress_bar_with_no_stats()
            {
                Component.FindComponent<EpicProgressBar>().Instance.Stats.ShouldBeNull();
            }

            public async Task When_epic_stats_becomes_available()
            {
                await Renderer.Dispatcher.InvokeAsync(() =>
                {
                    Meta.Stats = Stats = new EpicStats { Done = 1, InProgress = 2, NotStarted = 3 };
                });
            }

            public void Then_I_should_see_card_details_progress_bar_with_loaded_stats()
            {
                Component.FindComponent<EpicProgressBar>().Instance.Stats.ShouldBeSameAs(Stats);
            }

            public void When_I_click_on_the_card()
            {
                Component.Find("div.epic-card").Click();
            }

            public void Then_OnCardSelect_event_should_get_raised()
            {
                CardClicked.ShouldBe(Card);
            }

            public void When_I_should_see_the_selected_status()
            {
                Component.FindAll("div.epic-card.selected").ShouldNotBeEmpty();
            }
        }
    }
}