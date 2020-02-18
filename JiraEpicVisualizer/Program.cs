using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JiraEpicVisualizer
{

    class Program
    {
        private static readonly SemaphoreSlim Sem = new SemaphoreSlim(5);

        static async Task<int> Main(string[] args)
        {
            var cfg = LoadConfig();
            if (cfg?.AuthKey == null || cfg.JiraUri == null)
            {
                Console.Error.WriteLine("Please provide config.json with AuthKey and JiraUri");
                return 1;
            }
            Console.WriteLine($"Using {cfg.JiraUri}");

            using var client = CreateHttpClient(cfg);
            var epics = await QueryEpics(client, cfg);
            var csv = new EpicCsvWriter(cfg).Write(epics);

            Console.WriteLine();
            SaveRoadmap(csv);
            return 0;
        }

        private static async Task<Epic[]> QueryEpics(HttpClient client, Config cfg)
        {
            var query = "issuetype=Epic";
            if(!string.IsNullOrWhiteSpace(cfg.EpicQueryFilter))
                query+=$" AND ({cfg.EpicQueryFilter})";

            var response = await client.Query(query);
            var epics = ParseEpics(response, cfg.ProjectFilters);
            ShowProgress();
            await Task.WhenAll(epics.Select(i => GetEpicProgress(client, i)));
            return epics;
        }

        private static HttpClient CreateHttpClient(Config cfg)
        {
            var client = new HttpClient { BaseAddress = new Uri(cfg.JiraUri) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cfg.AuthKey)));
            client.DefaultRequestHeaders.Add("ContentType", "application/json");
            return client;
        }

        private static void SaveRoadmap(string csv)
        {
            var path = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}roadmap.txt";
            File.WriteAllText(path, csv);
            Console.WriteLine($"Roadmap saved to: {path}");
        }

        private static Config LoadConfig()
        {
            var path = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}config.json";
            return File.Exists(path)
                ? JsonSerializer.Deserialize<Config>(File.ReadAllText(path))
                : new Config();
        }

        private static async Task GetEpicProgress(HttpClient client, Epic issue)
        {
            await Sem.WaitAsync();
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
            finally { Sem.Release(); }
        }

        private static Epic[] ParseEpics(IReadOnlyList<JsonElement> elements, string[] projectFilters)
        {
            return elements.Select(ParseEpic)
                .Where(e => projectFilters.Any(f => e.Project.StartsWith(f)))
                .OrderBy(e => e.DueDate.GetValueOrDefault(DateTimeOffset.MaxValue))
                .ThenBy(e => e.Project)
                .ThenBy(e => e.Key)
                .ToArray();
        }

        private static Epic ParseEpic(JsonElement element)
        {
            var epic = new Epic
            {
                Id = element.GetProperty("id").GetString(),
                Key = element.GetProperty("key").GetString(),
                Project = element.GetProperty("fields").GetProperty("project").GetProperty("name").GetString(),
                ImageUrl = element.GetProperty("fields").GetProperty("project").GetProperty("avatarUrls").GetProperty("32x32").GetString(),
                Status = element.GetProperty("fields").GetProperty("status").GetProperty("statusCategory").GetProperty("name").GetString().ToPlainText(),
                Summary = element.GetProperty("fields").GetProperty("summary").GetString().ToPlainText(),
                Links = element.GetProperty("fields").GetProperty("issuelinks").EnumerateArray()
                .Where(l => l.TryGetProperty("outwardIssue", out _))
                .Select(ParseLink).ToArray()
            };
            var dueDateElement = element.GetProperty("fields").GetProperty("duedate");
            if (dueDateElement.ValueKind != JsonValueKind.Null)
                epic.DueDate = dueDateElement.GetDateTimeOffset();
            return epic;
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
    }
}
