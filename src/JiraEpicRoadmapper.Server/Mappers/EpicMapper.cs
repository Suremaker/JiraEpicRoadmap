using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JiraEpicRoadmapper.Contracts;
using Microsoft.Extensions.Options;

namespace JiraEpicRoadmapper.Server.Mappers
{
    public class EpicMapper : IEpicMapper
    {
        private readonly IOptions<Config> _config;

        public EpicMapper(IOptions<Config> config)
        {
            _config = config;
        }

        public Epic MapEpic(in JsonElement element, IReadOnlyDictionary<string, string[]> fieldsNameToKeyMap)
        {
            var fields = element.GetProperty("fields");
            var status = fields.GetProperty("status");
            var epic = new Epic
            {
                Id = element.GetProperty("id").GetString(),
                Key = element.GetProperty("key").GetString(),
                Project = fields.GetProperty("project").GetProperty("name").GetString(),
                Status = status.GetProperty("name").GetString(),
                StatusCategory = status.GetProperty("statusCategory").GetProperty("name").GetString(),
                Summary = fields.GetProperty("summary").GetString(),
                Links = fields.GetProperty("issuelinks").EnumerateArray()
                    .Where(l => l.TryGetProperty("outwardIssue", out _))
                    .Select(ParseLink).ToArray()
            };
            epic.Url = $"{_config.Value.JiraUri}/browse/{epic.Key}";

            epic.DueDate = GetDateTimeIfSet(fields, "duedate");
            epic.StartDate = GetFirstNotNullCustomFieldDateTime(fields, fieldsNameToKeyMap, "Start date");
            return epic;
        }

        private static Link ParseLink(JsonElement link)
        {
            return new Link
            {
                OutwardId = link.GetProperty("outwardIssue").GetProperty("id").GetString(),
                Type = link.GetProperty("type").GetProperty("outward").GetString()
            };
        }

        private DateTime? GetFirstNotNullCustomFieldDateTime(in JsonElement fields, IReadOnlyDictionary<string, string[]> fieldsNameToKeyMap, string propName)
        {
            if (!fieldsNameToKeyMap.TryGetValue(propName, out var propKeys))
                return null;
            foreach (var propKey in propKeys)
            {
                var date = GetDateTimeIfSet(fields, propKey);
                if (date != null)
                    return date;
            }

            return null;
        }

        private DateTime? GetDateTimeIfSet(in JsonElement fields, string propKey)
        {
            if (fields.TryGetProperty(propKey, out var prop) && prop.ValueKind != JsonValueKind.Null)
                return prop.GetDateTime();
            return null;
        }
    }
}