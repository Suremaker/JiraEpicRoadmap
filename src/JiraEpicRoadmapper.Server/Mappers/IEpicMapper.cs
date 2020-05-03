using System.Text.Json;
using JiraEpicRoadmapper.Contracts;
using Microsoft.Extensions.Options;

namespace JiraEpicRoadmapper.Server.Mappers
{
    public interface IEpicMapper
    {
        Epic MapEpic(in JsonElement element);
    }

    public class EpicMapper : IEpicMapper
    {
        private readonly IOptions<Config> _config;

        public EpicMapper(IOptions<Config> config)
        {
            _config = config;
        }

        public Epic MapEpic(in JsonElement element)
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
                Summary = fields.GetProperty("summary").GetString()
            };
            epic.Url = $"{_config.Value.JiraUri}/browse/{epic.Key}";

            return epic;
        }
    }
}