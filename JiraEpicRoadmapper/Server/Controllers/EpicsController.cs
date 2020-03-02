using JiraEpicRoadmapper.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JiraEpicRoadmapper.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EpicsController : ControllerBase
    {
        private static readonly SemaphoreSlim Sem = new SemaphoreSlim(5);
        private readonly IHttpClientFactory _clientFactory;
        private readonly Config _config;

        public EpicsController(IHttpClientFactory clientFactory, IOptionsSnapshot<Config> config)
        {
            _clientFactory = clientFactory;
            _config = config.Value;
        }

        [HttpGet]
        public async Task<Epic[]> Get()
        {
            var client = _clientFactory.CreateClient("jira");
            var fields = await client.GetJson("/rest/api/2/field");

            var query = "issuetype=Epic";
            if (!string.IsNullOrWhiteSpace(_config.EpicQueryFilter))
                query += $" AND ({_config.EpicQueryFilter})";

            var response = await client.Query(query);
            var epics = ParseEpics(response, _config.ProjectFilters, fields);
            // await Task.WhenAll(epics.Select(i => GetEpicProgress(client, i)));
            return CalculateTimes(epics);
        }

        private Epic[] CalculateTimes(Epic[] epics)
        {
            var today = DateTimeOffset.UtcNow.Date;

            foreach (var epic in epics)
            {
                epic.CalculatedStartDate = epic.StartDate.GetValueOrDefault(epic.DueDate.GetValueOrDefault(today.AddDays(1)).AddDays(-1));
                epic.CalculatedDueDate = epic.DueDate.GetValueOrDefault(epic.StartDate.GetValueOrDefault(today).AddDays(1));
                if (epic.CalculatedStartDate >= epic.CalculatedDueDate)
                    epic.CalculatedDueDate = epic.CalculatedStartDate.AddDays(1);
            }

            return epics;
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
            }
            finally { Sem.Release(); }
        }
        private Epic[] ParseEpics(IReadOnlyList<JsonElement> elements, string[] projectFilters,
            IReadOnlyDictionary<string, string[]> fields)
        {
            return elements.Select(e => ParseEpic(e, fields))
                .Where(e => projectFilters.Any(f => e.Project.StartsWith(f)))
                .OrderBy(e => e.DueDate.GetValueOrDefault(DateTimeOffset.MaxValue))
                .ThenBy(e => e.Project)
                .ThenBy(e => e.Key)
                .ToArray();
        }

        private Epic ParseEpic(JsonElement element, IReadOnlyDictionary<string, string[]> fieldMapper)
        {
            var fields = element.GetProperty("fields");
            var epic = new Epic
            {
                Id = element.GetProperty("id").GetString(),
                Key = element.GetProperty("key").GetString(),
                Project = fields.GetProperty("project").GetProperty("name").GetString(),
                ImageUrl = fields.GetProperty("project").GetProperty("avatarUrls").GetProperty("32x32").GetString(),
                Status = fields.GetProperty("status").GetProperty("statusCategory").GetProperty("name").GetString().ToPlainText(),
                Summary = fields.GetProperty("summary").GetString().ToPlainText(),
                Links = fields.GetProperty("issuelinks").EnumerateArray()
                    .Where(l => l.TryGetProperty("outwardIssue", out _))
                    .Select(ParseLink).ToArray()
            };
            epic.Url = $"{_config.JiraUri}/browse/{epic.Key}";

            if (fields.TryGetProperty("duedate", out var dueDateElement) && dueDateElement.ValueKind != JsonValueKind.Null)
                epic.DueDate = dueDateElement.GetDateTimeOffset();
            foreach (var field in fieldMapper.TryGetValue("Start date", out var flds) ? flds : Enumerable.Empty<string>())
                if (fields.TryGetProperty(field, out var startDateElement) && startDateElement.ValueKind != JsonValueKind.Null)
                    epic.StartDate = startDateElement.GetDateTimeOffset();
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
    }
}
