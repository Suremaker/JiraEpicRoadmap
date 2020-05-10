using System;
using System.Linq;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    public class LayoutDesignerTests
    {
        [Fact]
        public void It_should_order_epics_by_date_then_id_and_fill_them_in()
        {
            var epics = new[]
            {
                CreateMeta("5", "2020-05-06", "2020-05-15"),
                CreateMeta("4", "2020-05-06", "2020-05-10"),
                CreateMeta("3", "2020-05-10", "2020-05-16"),
                CreateMeta("2", "2020-05-16", "2020-05-18"),
                CreateMeta("1", "2020-05-16", "2020-05-19")
            };
            var designer = new LayoutDesigner();

            var result = designer.Layout(epics);
            result.SelectMany((x, i) => x.Select(y => $"{i}.{y.Epic.Id}"))
                .ShouldBe(new[] { "0.4", "0.3", "0.1", "1.5", "1.2" });
        }

        private EpicMetadata CreateMeta(string id, string start, string end)
        {
            return new EpicMetadata(new Epic { Id = id }, ToDay(start), ToDay(end));
        }

        private IndexedDay ToDay(string date)
        {
            var o = DateTimeOffset.Parse(date);
            return new IndexedDay(o, o.DayOfYear);
        }
    }
}
