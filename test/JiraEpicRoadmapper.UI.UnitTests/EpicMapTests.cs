using System;
using System.Linq;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Domain;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    public class EpicMapTests
    {
        private readonly Timeline _timeline = new Timeline(DateTimeOffset.Parse("2020-03-01"), DateTimeOffset.Parse("2020-03-31"), DateTimeOffset.Parse("2020-03-03"));

        [Theory]
        [InlineData("2020-03-04", "2020-03-14", "3:2020-03-04", "13:2020-03-14")]
        [InlineData("2020-03-04", "2020-03-04", "3:2020-03-04", "7:2020-03-08")]
        [InlineData("2020-03-04", null, "3:2020-03-04", "7:2020-03-08")]
        [InlineData(null, "2020-03-14", "9:2020-03-10", "13:2020-03-14")]
        [InlineData(null, null, "2:2020-03-03", "6:2020-03-07")]
        public void It_should_initialize_start_end_properties(string start, string end, string expectedStart, string expectedEnd)
        {
            var epic = new Epic
            {
                Id = "foo",
                StartDate = Utils.ToNullableDateTime(start),
                DueDate = Utils.ToNullableDateTime(end),
            };
            var map = EpicMap.Create(new[] { epic }, _timeline);
            var meta = map.Epics.Single();

            meta.Start.ToString().ShouldBe(expectedStart);
            meta.End.ToString().ShouldBe(expectedEnd);
        }

        [Fact]
        public void GetById_should_return_epic_by_Id()
        {
            var foo = new Epic { Id = "foo" };
            var bar = new Epic { Id = "bar" };
            var map = EpicMap.Create(new[] { foo, bar }, _timeline);
            map.TryGetById("foo").Epic.ShouldBeSameAs(foo);
            map.TryGetById("bar").Epic.ShouldBeSameAs(bar);
            map.TryGetById("baz").ShouldBeNull();
        }

        [Fact]
        public void It_should_initialize_metadata_with_known_inbounds_and_dependants()
        {
            var map = EpicMap.Create(new[]
            {
                new Epic {Id = "foo", Links = new[] {new Link {OutwardId = "bar"}, new Link {OutwardId = "baz"}}},
                new Epic {Id = "bar", Links = new[] {new Link {OutwardId = "baz"}}},
                new Epic {Id = "baz", Links = new[] {new Link {OutwardId = "unknown"}, new Link {OutwardId = "foo"}}}
            }, _timeline);

            var foo = map.TryGetById("foo");
            var bar = map.TryGetById("bar");
            var baz = map.TryGetById("baz");

            foo.Inbounds.ShouldBe(new[] { baz });
            foo.Dependants.ShouldBe(new[] { bar, baz });

            bar.Inbounds.ShouldBe(new[] { foo });
            bar.Dependants.ShouldBe(new[] { baz });

            baz.Inbounds.ShouldBe(new[] { foo, bar });
            baz.Dependants.ShouldBe(new[] { foo });
        }
    }
}
