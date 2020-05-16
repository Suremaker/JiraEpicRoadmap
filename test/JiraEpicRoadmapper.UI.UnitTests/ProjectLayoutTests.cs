using System;
using System.Linq;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Domain;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Services;
using Moq;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    public class ProjectLayoutTests
    {
        [Fact]
        public void Create_should_return_properly_initialized_layout()
        {
            const int projectRowIndex = 15;
            var inputEpics = new EpicMetadata[150];
            var designer = new Mock<ILayoutDesigner>();
            var laidOutEpics = new[]
            {
                new[] {CreateMeta("A"), CreateMeta("B")},
                new[] {CreateMeta("C")},
                new[] {CreateMeta("D")}
            };

            designer.Setup(d => d.Layout(inputEpics)).Returns(laidOutEpics);

            var layout = ProjectLayout.Create("foo", inputEpics, projectRowIndex, designer.Object);

            layout.Name.ShouldBe("foo");
            layout.ProjectRowIndex.ShouldBe(projectRowIndex);
            layout.LastRowIndex.ShouldBe(projectRowIndex + laidOutEpics.Length);
            layout.Epics.Select(e => $"{e.RowIndex}{e.Meta.Epic.Id}").ShouldBe(new[] { "16A", "16B", "17C", "18D" });
            layout.Epics.Select(e => e.Meta).ShouldBe(laidOutEpics.SelectMany(r => r));
        }

        private static EpicMetadata CreateMeta(string id)
        {
            return new EpicMetadata(new Epic { Id = id }, new IndexedDay(DateTimeOffset.MinValue, 1), new IndexedDay(DateTimeOffset.MinValue, 5));
        }
    }
}
