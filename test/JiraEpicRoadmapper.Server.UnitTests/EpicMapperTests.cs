using System.Text.Json;
using JiraEpicRoadmapper.Server.Mappers;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.Server.UnitTests
{
    public class EpicMapperTests
    {
        private readonly IEpicMapper _mapper;
        private readonly Config _config = new Config { JiraUri = "http://foo.bar" };

        public EpicMapperTests()
        {
            _mapper = new EpicMapper(new OptionsWrapper<Config>(_config));
        }

        [Fact]
        public void MapEpic_should_map_basic_epic_fields()
        {
            var key = "ABC-11";
            var id = "123";
            var summary = "My epic name";
            var project = "Project Name";
            var status = "Todo";
            var statusCategory = "To Do";

            var epicJson = $@"{{
    ""id"": ""{id}"",
    ""key"": ""{key}"",
    ""fields"": {{
        ""project"": {{
            ""name"": ""{project}""
        }},
        ""status"": {{
            ""name"": ""{status}"",
            ""statusCategory"": {{
                ""name"": ""{statusCategory}""
            }}
        }},
        ""summary"": ""{summary}""
    }}
}}";
            var epic = _mapper.MapEpic(ToJsonElement(epicJson));

            epic.Id.ShouldBe(id);
            epic.Key.ShouldBe(key);
            epic.Summary.ShouldBe(summary);
            epic.Project.ShouldBe(project);
            epic.Status.ShouldBe(status);
            epic.StatusCategory.ShouldBe(statusCategory);
            epic.Url.ShouldBe($"{_config.JiraUri}/browse/{key}");
        }

        private JsonElement ToJsonElement(string json) => JsonDocument.Parse(json).RootElement;
    }
}
