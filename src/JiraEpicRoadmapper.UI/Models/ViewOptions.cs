using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace JiraEpicRoadmapper.UI.Models
{
    public class ViewOptions : IViewOptions
    {
        private readonly NavigationManager _manager;

        public ViewOptions(NavigationManager manager)
        {
            _manager = manager;
            var query = QueryHelpers.ParseQuery(manager.ToAbsoluteUri(manager.Uri).Query);
            ShowClosed = IsSet(query, nameof(ShowClosed));
            ShowUnplanned = IsSet(query, nameof(ShowUnplanned));
            HideTodayIndicator = IsSet(query, nameof(HideTodayIndicator));
            HideCardDetails = IsSet(query, nameof(HideCardDetails));
        }

        public bool ShowClosed { get; private set; }
        public bool ShowUnplanned { get; private set; }
        public bool HideTodayIndicator { get; private set; }
        public bool HideCardDetails { get; private set; }

        public void ToggleClosed()
        {
            ShowClosed = !ShowClosed;
            UpdateNavigation();
        }

        public void ToggleUnplanned()
        {
            ShowUnplanned = !ShowUnplanned;
            UpdateNavigation();
        }

        public void ToggleCardDetails()
        {
            HideCardDetails = !HideCardDetails;
            UpdateNavigation();
        }

        public void ToggleTodayIndicator()
        {
            HideTodayIndicator = !HideTodayIndicator;
            UpdateNavigation();
        }

        public event Action OptionsChanged;

        private void UpdateNavigation()
        {
            var options = new Dictionary<string, string>();
            AddIfSet(options, nameof(ShowClosed), ShowClosed);
            AddIfSet(options, nameof(ShowUnplanned), ShowUnplanned);
            AddIfSet(options, nameof(HideTodayIndicator), HideTodayIndicator);
            AddIfSet(options, nameof(HideCardDetails), HideCardDetails);

            var builder = new UriBuilder(_manager.ToAbsoluteUri(_manager.Uri))
            {
                Query = string.Join('&', options.Select(o => $"{UrlEncoder.Default.Encode(o.Key)}={UrlEncoder.Default.Encode(o.Value)}"))
            };

            _manager.NavigateTo(builder.ToString());
            OptionsChanged?.Invoke();
        }

        private void AddIfSet(Dictionary<string, string> options, string name, in bool value)
        {
            if (value)
                options[name.ToLowerInvariant()] = "y";
        }

        private bool IsSet(Dictionary<string, StringValues> query, string name)
        {
            return (query.TryGetValue(name.ToLowerInvariant(), out var values)) &&
                   string.Equals(values.FirstOrDefault(), "y", StringComparison.OrdinalIgnoreCase);
        }
    }
}