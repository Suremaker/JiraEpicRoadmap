using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
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

            var response = await client.Query("/rest/api/2/search?jql=issuetype%3DEpic%20and%20status!%3Ddone");
            var issues = ParseIssues(response);
            ShowProgress();

            var csv = new StringBuilder(GetCsvTemplate());
            csv.AppendLine("id,key,project,status,summary,refs,fill,stroke,ticketUri");
            foreach (var issue in issues)
            {
                csv.AppendLine($"{issue.Id},{issue.Key},{issue.Project},{issue.Status},{issue.Summary},\"{string.Join(',', issue.Links.Select(l => l.OutwardId))}\",{ToFill(issue.Status)},{ToStroke(issue.Status)},https://bnuttall.atlassian.net/browse/{issue.Key}");
                ShowProgress();
            }

            File.WriteAllText($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}roadmap.txt", csv.ToString());
        }


        private static Issue[] ParseIssues(JsonDocument doc)
        {
            return doc.RootElement.GetProperty("issues").EnumerateArray().Select(issue => ParseIssue(issue)).ToArray();
        }

        private static Issue ParseIssue(JsonElement issue)
        {
            return new Issue
            {
                Id = issue.GetProperty("id").GetString(),
                Key = issue.GetProperty("key").GetString(),
                Project = issue.GetProperty("fields").GetProperty("project").GetProperty("name").GetString(),
                Status = issue.GetProperty("fields").GetProperty("status").GetProperty("name").GetString().ToPlainText(),
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

        private static string ToFill(string status)
        {
            if (status.Equals("in progress", StringComparison.OrdinalIgnoreCase))
                return "#aaaaff";
            if (status.Equals("done", StringComparison.OrdinalIgnoreCase))
                return "#aaffaa";
            return "#aaaaaa";
        }

        private static string ToStroke(string status)
        {
            if (status.Equals("in progress", StringComparison.OrdinalIgnoreCase))
                return "#6666ff";
            if (status.Equals("done", StringComparison.OrdinalIgnoreCase))
                return "#66ff66";
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
