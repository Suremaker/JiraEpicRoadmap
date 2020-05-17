using System;
using System.Collections.Generic;
using JiraEpicRoadmapper.UI.Models;

namespace JiraEpicRoadmapper.UI.Services
{
    public class StatusVisualizer : IStatusVisualizer
    {
        public IEnumerable<string> GetStatusIcons(EpicCard card)
        {
            var status = new List<string>();

            var epicStatus = card.Meta.Epic.StatusCategory;
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
