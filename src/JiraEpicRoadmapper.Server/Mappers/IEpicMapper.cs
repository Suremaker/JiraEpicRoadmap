using System.Collections.Generic;
using System.Text.Json;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.Server.Mappers
{
    public interface IEpicMapper
    {
        Epic MapEpic(in JsonElement element, IReadOnlyDictionary<string, string[]> fieldsNameToKeyMap);
    }
}