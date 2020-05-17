using System.Threading.Tasks;
using JiraEpicRoadmapper.UI.Shared.Components;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class LoadingBarTests : FeatureFixture
    {
        [Scenario]
        public async Task Bar_with_no_label()
        {
            await Runner
                .WithContext<LoadingBarFixture>()
                .AddSteps(
                    x => x.Given_a_loading_bar(),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"loading-bar panel\"></div>"))
                .RunAsync();
        }

        [Scenario]
        public async Task Bar_with_label()
        {
            await Runner
                .WithContext<LoadingBarFixture>()
                .AddSteps(
                    x => x.Given_a_loading_bar(),
                    x => x.Given_it_has_parameter_value(nameof(LoadingBar.Label), "test"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"loading-bar panel\">test</div>"))
                .RunAsync();
        }

        [Scenario]
        public async Task Inline_bar_with_label()
        {
            await Runner
                .WithContext<LoadingBarFixture>()
                .AddSteps(
                    x => x.Given_a_loading_bar(),
                    x => x.Given_it_has_parameter_value(nameof(LoadingBar.Label), "test"),
                    x => x.Given_it_has_parameter_value(nameof(LoadingBar.Inline), true),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"loading-bar\">test</div>"))
                .RunAsync();
        }

        public class LoadingBarFixture : ComponentFixture<LoadingBar>
        {
            public void Given_a_loading_bar()
            {
            }
        }
    }
}