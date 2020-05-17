using System.Threading.Tasks;
using Bunit;
using JiraEpicRoadmapper.UI.Shared.Components;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Shouldly;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class ButtonTests : FeatureFixture
    {
        [Scenario]
        public async Task Normal_button()
        {
            await Runner
                .WithContext<ButtonFixture>()
                .AddSteps(
                    x => x.Given_a_button(),
                    x => x.Given_it_has_parameter_value(nameof(Button.Text), "Hello"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"button\" style=\"visibility: visible; display: inline-block\">Hello</div>"),
                    x => x.When_I_click_it(),
                    x => x.Then_I_should_get_click_event())
                .RunAsync();
        }

        [Scenario]
        public async Task Collapsed_button()
        {
            await Runner
                .WithContext<ButtonFixture>()
                .AddSteps(
                    x => x.Given_a_button(),
                    x => x.Given_it_has_parameter_value(nameof(Button.Text), "Hello"),
                    x => x.Given_it_has_parameter_value(nameof(Button.Collapsed), true),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"button\" style=\"visibility: collapsed; display: none\">Hello</div>"))
                .RunAsync();
        }

        public class ButtonFixture : ComponentFixture<Button>
        {
            private bool _clicked;

            public void Given_a_button()
            {
                WithComponentParameter(EventCallback(nameof(Button.OnClick), () => _clicked = true));
            }

            public void When_I_click_it()
            {
                Component.Find("div.button").Click();
            }

            public void Then_I_should_get_click_event()
            {
                _clicked.ShouldBe(true);
            }
        }
    }
}