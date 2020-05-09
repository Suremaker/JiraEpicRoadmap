using Bunit;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;

namespace JiraEpicRoadmapper.UI.Tests.Components.Fixtures
{
    public class LoadingBarFixture : ComponentFixture<LoadingBar>
    {
        public void Given_a_loading_bar()
        {
        }

        public void I_should_see_content(string content)
        {
            Component.MarkupMatches(content);
        }
    }
}