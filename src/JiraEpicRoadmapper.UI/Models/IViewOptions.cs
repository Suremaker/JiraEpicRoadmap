using System;

namespace JiraEpicRoadmapper.UI.Models
{
    public interface IViewOptions
    {
        bool ShowClosed { get; }
        bool ShowUnplanned { get; }
        void ToggleClosed();
        void ToggleUnplanned();
        event Action OptionsChanged;
    }
}