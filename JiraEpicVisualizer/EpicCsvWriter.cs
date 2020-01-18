using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JiraEpicVisualizer
{
    internal class EpicCsvWriter
    {
        private readonly List<(string column, Func<Epic, string> formatter)> _columns = new List<(string column, Func<Epic, string> formatter)>();

        public EpicCsvWriter(Config cfg)
        {
            AddFormatter("id", e => e.Id);
            AddFormatter("key", e => e.Key);
            AddFormatter("project", e => e.Project);
            AddFormatter("summary", e => e.Summary);
            AddFormatter("refs", e => $"\"{string.Join(',', e.Links.Select(l => l.OutwardId))}\"");
            AddFormatter("fill", e => e.GetSeverityColor().ToRgb());
            AddFormatter("stroke", e => e.GetSeverityColor().ToBorderColor().ToRgb());
            AddFormatter("ticketUri", e => $"{cfg.JiraUri}/browse/{e.Key}");
            AddFormatter("ticketsTotal", e => $"{e.Stats.Total}");
            AddFormatter("ticketsInProgress", e => $"{e.Stats.InProgress}");
            AddFormatter("ticketsDone", e => $"{e.Stats.Done}");
            AddFormatter("ticketsNotStarted", e => $"{e.Stats.NotStarted}");
            AddFormatter("duedate", e => e.DueDate?.ToString("d") ?? "none");
            AddFormatter("image", e => e.ImageUrl);
            AddFormatter("progressBar", e => GetProgressBar(e.Stats));
        }

        public string Write(Epic[] epics)
        {
            var csv = new StringBuilder(GetCsvTemplate());

            csv.AppendLine(string.Join(',', _columns.Select(c => c.column)));

            foreach (var epic in epics)
            {
                var first = true;
                foreach (var c in _columns)
                {
                    if (first) first = false;
                    else csv.Append(',');
                    csv.Append(c.formatter(epic));
                }

                csv.AppendLine();
            }

            return csv.ToString();
        }

        private void AddFormatter(string column, Func<Epic, string> formatter) => _columns.Add((column, formatter));

        private static string GetCsvTemplate()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JiraEpicVisualizer.template.txt");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static string GetProgressBar(TicketStats s)
        {
            var sb = new StringBuilder("\"<div style=\\\"height:10px;");
            if (s.Total > 0)
            {
                sb.Append("background:linear-gradient(to right");
                var done = s.DonePercentage;
                var progress = s.InProgressPercentage + s.DonePercentage;

                if (done > 0)
                    sb.Append($", #50ff50 0% {done}%");

                if (progress > done)
                    sb.Append($", #5050ff {done}% {progress}%");

                if (progress < 100)
                    sb.Append($", #505050 {progress}% 100%");

                sb.Append(");");
            }
            sb.Append("\\\"> </div>\"");
            return sb.ToString();
        }
    }
}