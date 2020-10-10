using System;
using System.Collections.Generic;

namespace JiraEpicRoadmapper.UI.Models
{
    public interface IViewOptions
    {
        bool ShowClosed { get; }
        bool ShowUnplanned { get; }
        bool HideTodayIndicator { get; }
        bool HideCardDetails { get; }
        IReadOnlyList<string> SelectedProjects { get; }
        void ToggleSelectedProjects(string project);
        void ToggleClosed();
        void ToggleUnplanned();
        void ToggleCardDetails();
        void ToggleTodayIndicator();
        event Action OptionsChanged;
    }
}