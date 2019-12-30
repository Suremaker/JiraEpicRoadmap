using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JiraEpicVisualizer
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var cfg = JsonSerializer.Deserialize<Config>(File.ReadAllText($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}config.json"));

            using var client = new HttpClient() { BaseAddress = new Uri(cfg.JiraUri) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cfg.AuthKey)));
            client.DefaultRequestHeaders.Add("ContentType", "application/json");

            var response = await client.Query("issuetype=Epic and status!=done");
            var issues = ParseIssues(response);
            ShowProgress();
            await Task.WhenAll(issues.Select(i => GetEpicProgress(client, i)));

            var csv = new StringBuilder(GetCsvTemplate());
            csv.AppendLine("id,key,project,summary,refs,fill,stroke,ticketUri,total,inprogress,done,percentage");

            foreach (var issue in issues)
                csv.AppendLine($"{issue.Id},{issue.Key},{issue.Project},{issue.Summary},\"{string.Join(',', issue.Links.Select(l => l.OutwardId))}\",{ToFill(issue.Stats)},{ToStroke(issue.Stats)},{cfg.JiraUri}/browse/{issue.Key},{issue.Stats.Total},{issue.Stats.InProgress},{issue.Stats.Done},{issue.Stats.Percentage}");

            File.WriteAllText($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}roadmap.txt", csv.ToString());
        }

        private static readonly SemaphoreSlim sem = new SemaphoreSlim(5);

        private static async Task GetEpicProgress(HttpClient client, Issue issue)
        {
            await sem.WaitAsync();
            try
            {
                var result = await client.Query($"issuetype in standardIssueTypes() AND \"Epic Link\"={issue.Key}");
                var statuses = result.Select(t => t.GetProperty("fields").GetProperty("status").GetProperty("statusCategory").GetProperty("name").GetString().ToPlainText()).GroupBy(x => x);
                var stats = new TicketStats();
                foreach (var grp in statuses)
                {
                    if (grp.Key.Equals("done", StringComparison.OrdinalIgnoreCase))
                        stats.Done += grp.Count();
                    else if (grp.Key.Equals("in progress", StringComparison.OrdinalIgnoreCase))
                        stats.InProgress += grp.Count();
                    else 
                        stats.NotStarted += grp.Count();
                }
                issue.Stats = stats;
                ShowProgress();
            }
            finally { sem.Release(); }
        }

        private static Issue[] ParseIssues(IEnumerable<JsonElement> issues)
        {
            return issues.Select(issue => ParseIssue(issue)).ToArray();
        }

        private static Issue ParseIssue(JsonElement issue)
        {
            return new Issue
            {
                Id = issue.GetProperty("id").GetString(),
                Key = issue.GetProperty("key").GetString(),
                Project = issue.GetProperty("fields").GetProperty("project").GetProperty("name").GetString(),
                Status = issue.GetProperty("fields").GetProperty("status").GetProperty("statusCategory").GetProperty("name").GetString().ToPlainText(),
                Summary = issue.GetProperty("fields").GetProperty("summary").GetString().ToPlainText(),
                Links = issue.GetProperty("fields").GetProperty("issuelinks").EnumerateArray()
                            .Where(l => l.TryGetProperty("outwardIssue", out _))
                            .Select(link => ParseLink(link)).ToArray()
            };
        }

        private static Link ParseLink(JsonElement link)
        {
            return new Link
            {
                OutwardId = link.GetProperty("outwardIssue").GetProperty("id").GetString(),
                Type = link.GetProperty("type").GetProperty("name").GetString()
            };
        }

        private static void ShowProgress()
        {
            Console.Write('.');
        }

        private static string ToFill(TicketStats stats)
        {
            if (stats.Total == 0)
                return "#888888";
            if (stats.Done == stats.Total)
                return "#aaffaa";
            if (stats.NotStarted < stats.Total)
                return "#aaaaff";
            return "#aaaaaa";
        }

        private static string ToStroke(TicketStats stats)
        {
            if (stats.Total == 0)
                return "#444444";

            if (stats.Done == stats.Total)
                return "#66ff66";

            if (stats.NotStarted < stats.Total)
                return "#6666ff";

            return "#666666";
        }

        private static string GetCsvTemplate()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JiraEpicVisualizer.template.txt");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
