using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.UI.Models
{
    public class EpicMetadata
    {
        private readonly List<EpicMetadata> _inbounds = new List<EpicMetadata>();
        private IReadOnlyList<EpicMetadata> _dependants;
        public Epic Epic { get; }
        public IndexedDay Start { get; }
        public IndexedDay End { get; }
        public IReadOnlyList<EpicMetadata> Dependants => _dependants ?? Array.Empty<EpicMetadata>();
        public IReadOnlyList<EpicMetadata> Inbounds => _inbounds;

        public EpicMetadata(Epic epic, IndexedDay start, IndexedDay end)
        {
            Epic = epic;
            Start = start;
            End = end;
        }

        public void Initialize(EpicMap map)
        {
            foreach (var e in GetDependents(map))
                e._inbounds.Add(this);
        }

        private IReadOnlyList<EpicMetadata> GetDependents(EpicMap map)
        {
            if (_dependants != null)
                return _dependants;

            return _dependants = Epic.Links
                .Select(l => map.TryGetById(l.OutwardId))
                .Where(e => e != null)
                .ToArray();
        }
    }
}
