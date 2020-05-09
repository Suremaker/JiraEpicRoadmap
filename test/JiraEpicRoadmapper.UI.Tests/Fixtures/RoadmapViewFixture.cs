using Bunit;
using JiraEpicRoadmapper.UI.Pages;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using Shouldly;

namespace JiraEpicRoadmapper.UI.Tests.Fixtures
{
    public class RoadmapViewFixture : ComponentFixture<RoadmapView>
    {
        public void Given_a_freshly_opened_page()
        {
        }

        public void I_should_see_progress_bar_with_text(string text)
        {
            Component.FindComponent<LoadingBar>().Instance.Label.ShouldBe(text);
        }
    }
}