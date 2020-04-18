﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace JiraEpicRoadmapper.Client.Model
{
    public class ViewOptions
    {
        private readonly NavigationManager _manager;

        public ViewOptions(NavigationManager manager)
        {
            _manager = manager;
            var query = QueryHelpers.ParseQuery(manager.ToAbsoluteUri(manager.Uri).Query);
            CompactView = IsSet(query, nameof(CompactView));
            HideClosedEpics = IsSet(query, nameof(HideClosedEpics));
        }

        public bool CompactView { get; private set; }
        public bool HideClosedEpics { get; private set; }

        public void ToggleShowDependencies()
        {
            CompactView = !CompactView;
            UpdateNavigation();
        }

        public void ToggleClosedEpics()
        {
            HideClosedEpics = !HideClosedEpics;
            UpdateNavigation();
        }

        private void UpdateNavigation()
        {
            var options = new Dictionary<string, string>();
            AddIfSet(options, nameof(CompactView), CompactView);
            AddIfSet(options, nameof(HideClosedEpics), HideClosedEpics);

            var builder = new UriBuilder(_manager.ToAbsoluteUri(_manager.Uri))
            {
                Query = string.Join('&', options.Select(o => $"{UrlEncoder.Default.Encode(o.Key)}={UrlEncoder.Default.Encode(o.Value)}"))
            };

            _manager.NavigateTo(builder.ToString());
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