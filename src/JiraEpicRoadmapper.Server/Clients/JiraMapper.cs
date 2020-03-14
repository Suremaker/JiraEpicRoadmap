using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JiraEpicRoadmapper.Models;
using Microsoft.Extensions.Options;

namespace JiraEpicRoadmapper.Server.Clients
{
    public class JiraMapper
    {
        private readonly Config _config;

        public JiraMapper(IOptions<Config> config)
        {
            _config = config.Value;
        }

        public Epic[] ParseEpics(IReadOnlyList<JsonElement> elements, IReadOnlyDictionary<string, string[]> fieldMap)
        {
            return elements.Select(e => ParseEpic(e, fieldMap)).ToArray();
        }

        private Epic ParseEpic(JsonElement element, IReadOnlyDictionary<string, string[]> fieldMapper)
        {
            var fields = element.GetProperty("fields");
            var epic = new Epic
            {
                Id = element.GetProperty("id").GetString(),
                Key = element.GetProperty("key").GetString(),
                Project = fields.GetProperty("project").GetProperty("name").GetString(),
                Status = fields.GetProperty("status").GetProperty("statusCategory").GetProperty("name").GetString(),
                Summary = fields.GetProperty("summary").GetString(),
                Links = fields.GetProperty("issuelinks").EnumerateArray()
                    .Where(l => l.TryGetProperty("outwardIssue", out _))
                    .Select(ParseLink).ToArray()
            };
            epic.Url = $"{_config.JiraUri}/browse/{epic.Key}";

            if (fields.TryGetProperty("duedate", out var dueDateElement) && dueDateElement.ValueKind != JsonValueKind.Null)
                epic.DueDate = dueDateElement.GetDateTimeOffset();

            foreach (var field in fieldMapper.TryGetValue("Start date", out var flds) ? flds : Enumerable.Empty<string>())
            {
                if (fields.TryGetProperty(field, out var startDateElement) && startDateElement.ValueKind != JsonValueKind.Null)
                    epic.StartDate = startDateElement.GetDateTimeOffset();
            }

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