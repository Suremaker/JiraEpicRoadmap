using System;
using System.Collections.Generic;

namespace JiraEpicRoadmapper.UI.Models
{
    public class StatusVisualizer : IStatusVisualizer
    {
        public IEnumerable<string> GetStatusIcons(EpicVisualBlock block)
        {
            var status = new List<string>();

            var epicStatus = block.Meta.Epic.StatusCategory;
            if (string.Equals(epicStatus, "done", StringComparison.OrdinalIgnoreCase))
                status.Add("✔️");
            else if (string.Equals(epicStatus, "in progress", StringComparison.OrdinalIgnoreCase))
                status.Add("⚙️");
            else
                status.Add("❔");

            return status;
        }
    }
}
