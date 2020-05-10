using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using Moq;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    public class EpicsRoadmapTests
    {
        private readonly Mock<ILayoutDesigner> _designer;

        public EpicsRoadmapTests()
        {
            _designer = new Mock<ILayoutDesigner>();
            _designer
                .Setup(x => x.Layout(It.IsAny<IEnumerable<EpicMetadata>>()))
                .Returns((IEnumerable<EpicMetadata> input) => new[] { input });
        }


        [Fact]
        public void It_should_return_roadmap_and_map()
        {
            var epic = new Epic { Id = "foo" };
            var roadmap = new EpicsRoadmap(new[] { epic });

            roadmap.Timeline.ShouldNotBeNull();
            roadmap.Map.ShouldNotBeNull();
            roadmap.Map.TryGetById("foo").ShouldNotBeNull();
            roadmap.Projects.ShouldNotBeNull();
        }

        [Fact]
        public void UpdateLayout_should_layout_epics()
        {
            var epics = new[]
            {
                new Epic {Id = "PR-1", Project = "PR"},
                new Epic {Id = "XX-1", Project = "XX"},
                new Epic {Id = "PR-2", Project = "PR"},
            };

            var roadmap = new EpicsRoadmap(epics);

            
            roadmap.UpdateLayout(_designer.Object);

            roadmap.Projects.Count.ShouldBe(2);
            roadmap.Projects[0].Name.ShouldBe("PR");
            roadmap.Projects[0].ProjectRowIndex.ShouldBe(2);
            roadmap.Projects[0].LastRowIndex.ShouldBe(3);
            roadmap.Projects[0].Epics.Select(e=>e.Meta.Epic.Id).ShouldBe(new[]{"PR-1","PR-2"});

            roadmap.Projects[1].Name.ShouldBe("XX");
            roadmap.Projects[1].ProjectRowIndex.ShouldBe(4);
            roadmap.Projects[1].LastRowIndex.ShouldBe(5);
            roadmap.Projects[1].Epics.Select(e => e.Meta.Epic.Id).ShouldBe(new[] { "XX-1"});

            roadmap.TotalRows.ShouldBe(6);
        }
    }
}
