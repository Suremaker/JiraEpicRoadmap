using System;
using JiraEpicRoadmapper.UI.Models;

namespace JiraEpicRoadmapper.UI.Tests.Scaffolding
{
    class TestableViewOptions : IViewOptions
    {
        public bool ShowClosed { get; set; }
        public bool ShowUnplanned { get; set; }
        public bool HideTodayIndicator { get; set; }
        public bool HideCardDetails { get; set; }

        public void ToggleClosed()
        {
            ShowClosed = !ShowClosed;
            OptionsChanged?.Invoke();
        }

        public void ToggleUnplanned()
        {
            ShowUnplanned = !ShowUnplanned;
            OptionsChanged?.Invoke();
        }

        public void ToggleCardDetails()
        {
            HideCardDetails = !HideCardDetails;
            OptionsChanged?.Invoke();
        }

        public void ToggleTodayIndicator()
        {
            HideTodayIndicator = !HideTodayIndicator;
            OptionsChanged?.Invoke();
        }

        public event Action OptionsChanged;
    }
}