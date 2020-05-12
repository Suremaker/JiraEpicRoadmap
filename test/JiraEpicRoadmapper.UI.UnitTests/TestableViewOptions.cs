using System;
using JiraEpicRoadmapper.UI.Models;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    class TestableViewOptions : IViewOptions
    {
        public bool ShowClosed { get; set; }
        public bool ShowUnplanned { get; set; }
        public void ToggleClosed()
        {
            ShowClosed = !ShowClosed;
        }

        public void ToggleUnplanned()
        {
            ShowUnplanned = !ShowUnplanned;
        }

        public event Action OptionsChanged;
    }
}