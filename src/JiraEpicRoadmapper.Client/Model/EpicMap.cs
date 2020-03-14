using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Models;

namespace JiraEpicRoadmapper.Client.Model
{
    public class EpicMap
    {
        private readonly IReadOnlyDictionary<string, EpicBlock> _epics;

        public EpicMap(IEnumerable<Epic> epics, Func<DateTimeOffset?, int> dateToIndexMapper)
        {
            _epics = epics.ToDictionary(e => e.Id, e => new EpicBlock(e, dateToIndexMapper(e.StartDate ?? e.DueDate), dateToIndexMapper(e.DueDate ?? e.StartDate)));

            foreach (var e in _epics.Values)
                e.Initialize(this);
        }

        public IEnumerable<EpicBlock> Epics => _epics.Values;


        public EpicBlock TryGetById(string id) => _epics.TryGetValue(id, out var e) ? e : null;
    }
}