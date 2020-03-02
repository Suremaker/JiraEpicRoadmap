using System.Collections.Generic;
using JiraEpicRoadmapper.Shared;

namespace JiraEpicRoadmapper.Client
{
    public class ProjectLayout
    {
        public string Name { get; }
        public IReadOnlyList<IReadOnlyList<Epic>> Epics { get; }

        public ProjectLayout(string name, IReadOnlyList<IReadOnlyList<Epic>> epics)
        {
            Name = name;
            Epics = epics;
        }
    }
}