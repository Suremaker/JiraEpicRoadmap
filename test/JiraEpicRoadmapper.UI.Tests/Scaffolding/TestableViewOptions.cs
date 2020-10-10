using System;
using System.Collections.Generic;
using JiraEpicRoadmapper.UI.Models;

namespace JiraEpicRoadmapper.UI.Tests.Scaffolding
{
    class TestableViewOptions : IViewOptions
    {
        private readonly List<string> _selectedProjects = new List<string>();
        public bool ShowClosed { get; set; }
        public bool ShowUnplanned { get; set; }
        public bool HideTodayIndicator { get; set; }
        public bool HideCardDetails { get; set; }
        public IReadOnlyList<string> SelectedProjects => _selectedProjects;

        public void ToggleSelectedProjects(string project)
        {
            project = project.ToLowerInvariant();
            if (!_selectedProjects.Remove(project))
                _selectedProjects.Add(project);
            OptionsChanged?.Invoke();
        }

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