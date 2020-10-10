using System.Threading.Tasks;
using Bunit;
using JiraEpicRoadmapper.UI.Shared.Components;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Shouldly;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class ToggleOptionTests : FeatureFixture
    {
        [Scenario]
        public async Task Unchecked_option()
        {
            await Runner
                .WithContext<ToggleOptionFixture>()
                .AddSteps(
                    x => x.Given_a_toggle_option(),
                    x => x.Given_it_has_parameter_value(nameof(ToggleOption.Text), "Hello"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"toggle-option\" style=\"visibility: visible; display: block\">❌ Hello</div>"),
                    x => x.When_I_click_it(),
                    x => x.Then_I_should_get_click_event())
                .RunAsync();
        }

        [Scenario]
        public async Task Checked_option()
        {
            await Runner
                .WithContext<ToggleOptionFixture>()
                .AddSteps(
                    x => x.Given_a_toggle_option(),
                    x => x.Given_it_has_parameter_value(nameof(ToggleOption.Text), "Hello"),
                    x => x.Given_it_has_parameter_value(nameof(ToggleOption.Checked), true),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"toggle-option\" style=\"visibility: visible; display: block\">✔️ Hello</div>"))
                .RunAsync();
        }

        [Scenario]
        public async Task Collapsed_option()
        {
            await Runner
                .WithContext<ToggleOptionFixture>()
                .AddSteps(
                    x => x.Given_a_toggle_option(),
                    x => x.Given_it_has_parameter_value(nameof(Button.Text), "Hello"),
                    x => x.Given_it_has_parameter_value(nameof(Button.Collapsed), true),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"toggle-option\" style=\"visibility: collapsed; display: none\">❌ Hello</div>"))
                .RunAsync();
        }

        public class ToggleOptionFixture : ComponentFixture<ToggleOption>
        {
            private bool _clicked;

            public void Given_a_toggle_option()
            {
                WithComponentParameter(EventCallback(nameof(ToggleOption.OnClick), (bool x) => _clicked = true));
            }

            public void When_I_click_it()
            {
                Component.Find("div.toggle-option").Click();
            }

            public void Then_I_should_get_click_event()
            {
                _clicked.ShouldBe(true);
            }
        }
    }
}