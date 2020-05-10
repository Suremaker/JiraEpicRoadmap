using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.UI.Models
{
    public class EpicMap
    {
        private readonly Dictionary<string, EpicMetadata> _epics;
        public IEnumerable<EpicMetadata> Epics => _epics.Values;

        public EpicMetadata TryGetById(string id) => _epics.TryGetValue(id, out var r) ? r : null;
        public static EpicMap Create(IReadOnlyList<Epic> epics, Timeline timeline) => new EpicMap(epics.Select(e => ToMetadata(e, timeline)));

        private EpicMap(IEnumerable<EpicMetadata> epics)
        {
            _epics = epics.ToDictionary(e => e.Epic.Id);
            foreach (var e in _epics.Values)
                e.Initialize(this);
        }

        private static EpicMetadata ToMetadata(Epic epic, Timeline timeline)
        {
            var start = epic.StartDate ?? epic.DueDate?.AddDays(-LayoutSettings.MinEpicBlockLength) ?? timeline.Today.Date;
            var end = epic.DueDate ?? epic.StartDate ?? timeline.Today.Date;
            if (end - start < TimeSpan.FromDays(LayoutSettings.MinEpicBlockLength))
                end = start + TimeSpan.FromDays(LayoutSettings.MinEpicBlockLength);

            return new EpicMetadata(epic, timeline.GetDayWithIndex(start), timeline.GetDayWithIndex(end));
        }
    }
}