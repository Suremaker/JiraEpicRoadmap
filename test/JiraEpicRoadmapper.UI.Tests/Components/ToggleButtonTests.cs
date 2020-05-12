using System.Threading.Tasks;
using Bunit;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Shouldly;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class ToggleButtonTests : FeatureFixture
    {
        [Scenario]
        public async Task Unchecked_button()
        {
            await Runner
                .WithContext<ToggleButtonFixture>()
                .AddSteps(
                    x => x.Given_a_toggle_button(),
                    x => x.Given_it_has_parameter_value(nameof(ToggleButton.UncheckedText), "Hello"),
                    x => x.Given_it_has_parameter_value(nameof(ToggleButton.CheckedText), "Bye"),
                    x => x.Given_it_has_parameter_value(nameof(ToggleButton.Checked), false),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"button\" style=\"visibility: visible; display: inline-block\">Hello</div>"),
                    x => x.When_I_click_it(),
                    x => x.Then_I_should_get_click_event_with_checked_flag(true))
                .RunAsync();
        }

        [Scenario]
        public async Task Checked_button()
        {
            await Runner
                .WithContext<ToggleButtonFixture>()
                .AddSteps(
                    x => x.Given_a_toggle_button(),
                    x => x.Given_it_has_parameter_value(nameof(ToggleButton.UncheckedText), "Hello"),
                    x => x.Given_it_has_parameter_value(nameof(ToggleButton.CheckedText), "Bye"),
                    x => x.Given_it_has_parameter_value(nameof(ToggleButton.Checked), true),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"button\" style=\"visibility: visible; display: inline-block\">Bye</div>"),
                    x => x.When_I_click_it(),
                    x => x.Then_I_should_get_click_event_with_checked_flag(false))
                .RunAsync();
        }

        [Scenario]
        public async Task Collapsed_button()
        {
            await Runner
                .WithContext<ToggleButtonFixture>()
                .AddSteps(
                    x => x.Given_a_toggle_button(),
                    x => x.Given_it_has_parameter_value(nameof(ToggleButton.UncheckedText), "Hello"),
                    x => x.Given_it_has_parameter_value(nameof(ToggleButton.Collapsed), true),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"button\" style=\"visibility: collapsed; display: none\">Hello</div>"))
                .RunAsync();
        }

        public class ToggleButtonFixture : ComponentFixture<ToggleButton>
        {
            private bool _clicked;
            private bool _checked;

            public void Given_a_toggle_button()
            {
                WithComponentParameter(EventCallback<bool>(nameof(ToggleButton.OnClick), x =>
                {
                    _clicked = true;
                    _checked = x;
                }));
            }

            public void When_I_click_it()
            {
                Component.Find("div.button").Click();
            }

            public void Then_I_should_get_click_event_with_checked_flag(bool flag)
            {
                _clicked.ShouldBe(true);
                _checked.ShouldBe(flag);
            }
        }
    }
}