using System;
using System.Drawing;
using JiraEpicVisualizer;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapTests
{
    public class ColorTests
    {
        [Theory]
        [InlineData(0x010203, "#010203")]
        [InlineData(0xaabbcc, "#aabbcc")]
        public void ToRgb_should_format_color_properly(int rgb, string expected)
        {
            Color.FromArgb(rgb).ToRgb().ShouldBe(expected);
        }

        [Theory]
        [InlineData(0x00ff00, 0xff0080, 0.5, "#7f7f40")]
        public void Morph_should_transform_color(int fromRgb, int toRgb, double progress, string expected)
        {
            Color.FromArgb(fromRgb).MorphTo(Color.FromArgb(toRgb), progress).ToRgb().ShouldBe(expected);
        }

        [Theory]
        [InlineData(90, "#aaaaaa")]
        [InlineData(60, "#aaffaa")]
        [InlineData(30, "#ffffaa")]
        [InlineData(15, "#ffff30")]
        [InlineData(0, "#ff3030")]
        [InlineData(5, "#ff7530")]
        public void GetSeverityColor_should_return_proper_color(int daysDue, string expected)
        {
            new Epic { DueDate = DateTimeOffset.UtcNow.AddDays(daysDue) }.GetSeverityColor().ToRgb().ShouldBe(expected);
        }

        [Theory]
        [InlineData(0xff8032,"#994c1e")]
        public void ToBorder_should_darker_the_color(int rgb, string expected)
        {
            Color.FromArgb(rgb).ToBorder().ToRgb().ShouldBe(expected);
        }
    }
}
