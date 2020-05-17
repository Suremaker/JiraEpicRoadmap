using System;
using JiraEpicRoadmapper.UI.Models;
using Microsoft.AspNetCore.Components;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    public class ViewOptionsTests
    {
        private readonly TestableNavigator _navigator;

        public ViewOptionsTests()
        {
            _navigator = new TestableNavigator();
        }

        [Fact]
        public void Toggling_options_should_initiate_redirect()
        {
            _navigator.Initialize("http://localhost/");
            var options = new ViewOptions(_navigator);
            options.ToggleClosed();
            _navigator.LastUri.ShouldBe("http://localhost:80/?showclosed=y");
            options.ToggleUnplanned();
            _navigator.LastUri.ShouldBe("http://localhost:80/?showclosed=y&showunplanned=y");
            options.ToggleClosed();
            _navigator.LastUri.ShouldBe("http://localhost:80/?showunplanned=y");
            options.ToggleUnplanned();
            _navigator.LastUri.ShouldBe("http://localhost:80/");
            options.ToggleTodayIndicator();
            _navigator.LastUri.ShouldBe("http://localhost:80/?hidetodayindicator=y");
            options.ToggleCardDetails();
            _navigator.LastUri.ShouldBe("http://localhost:80/?hidetodayindicator=y&hidecarddetails=y");
            options.ToggleCardDetails();
            options.ToggleTodayIndicator();
            _navigator.LastUri.ShouldBe("http://localhost:80/");
        }

        [Theory]
        [InlineData("http://localhost/?showclosed=y&showunplanned=y&hidetodayindicator=y&hidecarddetails=y", true, true, true, true)]
        [InlineData("http://localhost/?showclosed=y&showunplanned=y", true, true, false, false)]
        [InlineData("http://localhost/?showclosed=y", true, false, false, false)]
        [InlineData("http://localhost/", false, false, false, false)]
        public void It_should_preset_fields(string uri, bool showClosed, bool showUnplanned, bool hideToday, bool hideDetails)
        {
            _navigator.Initialize(uri);
            var options = new ViewOptions(_navigator);
            options.ShowUnplanned.ShouldBe(showUnplanned);
            options.ShowClosed.ShouldBe(showClosed);
        }

        [Fact]
        public void Toggle_should_trigger()
        {
            _navigator.Initialize("http://localhost/");
            var options = new ViewOptions(_navigator);
            var counter = 0;
            options.OptionsChanged += () => counter++;

            options.ToggleUnplanned();
            counter.ShouldBe(1);
            options.ToggleClosed();
            counter.ShouldBe(2);
            options.ToggleTodayIndicator();
            counter.ShouldBe(3);
            options.ToggleCardDetails();
            counter.ShouldBe(4);
        }

        private class TestableNavigator : NavigationManager
        {
            public void Initialize(string uri) => Initialize(new Uri(new Uri(uri), "/").ToString(), uri);
            protected override void NavigateToCore(string uri, bool forceLoad)
            {
                LastUri = uri;
            }

            public string LastUri { get; private set; }
        }
    }
}
