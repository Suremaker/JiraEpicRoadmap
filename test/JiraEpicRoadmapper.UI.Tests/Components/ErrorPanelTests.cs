using System.Threading.Tasks;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class ErrorPanelTests : FeatureFixture
    {
        [Scenario]
        public async Task Bar_with_no_label()
        {
            await Runner
                .WithContext<ErrorPanelFixture>()
                .AddSteps(
                    x => x.Given_a_error_panel(),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"error-panel\"></div>"))
                .RunAsync();
        }

        [Scenario]
        public async Task Bar_with_label()
        {
            await Runner
                .WithContext<ErrorPanelFixture>()
                .AddSteps(
                    x => x.Given_a_error_panel(),
                    x => x.Given_it_has_parameter_value(nameof(ErrorPanel.Errors), "boom"),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"error-panel\">boom</div>"))
                .RunAsync();
        }

        public class ErrorPanelFixture : ComponentFixture<ErrorPanel>
        {
            public void Given_a_error_panel()
            {
            }
        }
    }
}