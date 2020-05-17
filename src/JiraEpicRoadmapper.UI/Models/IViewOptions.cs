using System;

namespace JiraEpicRoadmapper.UI.Models
{
    public interface IViewOptions
    {
        bool ShowClosed { get; }
        bool ShowUnplanned { get; }
        bool HideTodayIndicator { get; }
        bool HideCardDetails { get; }
        void ToggleClosed();
        void ToggleUnplanned();
        void ToggleCardDetails();
        void ToggleTodayIndicator();
        event Action OptionsChanged;
    }
}