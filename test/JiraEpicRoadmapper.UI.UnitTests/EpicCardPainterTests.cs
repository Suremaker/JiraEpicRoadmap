using System.Globalization;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    public class EpicCardPainterTests
    {
        private readonly IEpicCardPainter _painter = new EpicCardPainter();

        [Fact]
        public void It_should_calculate_color_no_darker_than_100_RGB_value()
        {
            for (int i = 0; i < 100; ++i)
            {
                var color = _painter.GetColor(CreateVisualBlock(i.ToString()));
                var r = byte.Parse(color.Substring(1,2),NumberStyles.HexNumber);
                var g = byte.Parse(color.Substring(3,2), NumberStyles.HexNumber);
                var b = byte.Parse(color.Substring(5,2), NumberStyles.HexNumber);

                r.ShouldBeGreaterThanOrEqualTo((byte)100,$"R failed for {i}");
                g.ShouldBeGreaterThanOrEqualTo((byte)100,$"R failed for {i}");
                b.ShouldBeGreaterThanOrEqualTo((byte)100,$"R failed for {i}");
            }
        }

        [Fact]
        public void It_should_allow_calculating_color_for_null_project()
        {
            _painter.GetColor(CreateVisualBlock(null)).ShouldNotBeNull();
        }

        [Fact]
        public void It_should_return_html_style_color()
        {
            _painter.GetColor(CreateVisualBlock("PR1")).ShouldMatch("^#[0-9a-f]{6}$");
        }

        private EpicVisualBlock CreateVisualBlock(string projectName) => new EpicVisualBlock(new EpicMetadata(new Epic{Project = projectName},new IndexedDay(), new IndexedDay() ),1 );
    }
}
