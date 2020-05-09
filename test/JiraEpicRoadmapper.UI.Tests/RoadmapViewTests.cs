using System.Threading.Tasks;
using JiraEpicRoadmapper.UI.Tests.Fixtures;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;

namespace JiraEpicRoadmapper.UI.Tests
{
    public class RoadmapViewTests : FeatureFixture
    {
        [Scenario]
        public async Task Loading_page()
        {
            await Runner
                .WithContext<RoadmapViewFixture>()
                .AddSteps(
                    x => x.Given_a_freshly_opened_page(),
                    x => x.When_I_render_it(),
                    x => x.I_should_see_progress_bar_with_text("Loading..."))
                .RunAsync();
        }
    }
}
