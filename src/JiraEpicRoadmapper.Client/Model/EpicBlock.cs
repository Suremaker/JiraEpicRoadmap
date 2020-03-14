using System;
using System.Collections.Generic;
using System.Linq;
using JiraEpicRoadmapper.Models;

namespace JiraEpicRoadmapper.Client.Model
{
    public static class LayoutSettings
    {
        public const int MinBlockLength = 3;
    }
    public class EpicBlock
    {
        private EpicBlock[] _dependents;
        private readonly List<EpicBlock> _inbounds=new List<EpicBlock>();
        public Epic Epic { get; }
        public TicketStats Stats { get; set; }
        public int Row { get; set; } = -1;
        public int StartIndex { get; }
        public int EndIndex { get; }
        public int Depth { get; private set; } = -1;
        public int InboundRefs => _inbounds.Count;
        public IReadOnlyList<EpicBlock> Dependents => _dependents ?? Array.Empty<EpicBlock>();
        public IReadOnlyList<EpicBlock> Inbounds => _inbounds;
        public int NotLayoutParents => Inbounds.Count(p => p.Row < 0);
        public string Id => Epic.Id;

        public EpicBlock(Epic epic, int startIndex, int endIndex)
        {
            Epic = epic;
            StartIndex = startIndex;
            EndIndex = Math.Max(endIndex, startIndex + LayoutSettings.MinBlockLength);
        }

        public void Initialize(EpicMap map)
        {
            CalculateDepth(map);
            foreach (var e in GetDependents(map))
                e._inbounds.Add(this);
        }

        private EpicBlock CalculateDepth(EpicMap map)
        {
            if (Depth < 0)
            {
                Depth = GetDependents(map)
                    .Select(e => (int?)e.CalculateDepth(map).Depth)
                    .OrderByDescending(x => x)
                    .FirstOrDefault()
                    .GetValueOrDefault(-1) + 1;
            }
            return this;
        }

        private IReadOnlyList<EpicBlock> GetDependents(EpicMap map)
        {
            if (_dependents != null)
                return _dependents;

            return _dependents = Epic.Links
                .Select(l => map.TryGetById(l.OutwardId))
                .Where(e => e != null)
                .ToArray();
        }
        public bool Overlaps(EpicBlock e) => !(EndIndex <= e.StartIndex || e.EndIndex <= StartIndex);

        public bool DependsOn(EpicBlock e) => e.Dependents.Contains(this);
    }
}
