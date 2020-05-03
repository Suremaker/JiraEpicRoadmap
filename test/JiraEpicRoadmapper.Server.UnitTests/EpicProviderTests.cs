using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.Server.Clients;
using JiraEpicRoadmapper.Server.Mappers;
using JiraEpicRoadmapper.Server.Providers;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.Server.UnitTests
{
    public class EpicProviderTests
    {
        private readonly Mock<IJiraClient> _client = new Mock<IJiraClient>();
        private readonly Mock<IEpicMapper> _mapper = new Mock<IEpicMapper>();
        private readonly Config _config = new Config();
        private readonly IEpicProvider _provider;

        public EpicProviderTests()
        {
            _provider = new EpicProvider(_client.Object, _mapper.Object, new OptionsWrapper<Config>(_config));
            SetupClient();
        }

        [Fact]
        public async Task GetEpics_should_query_for_epics()
        {
            await _provider.GetEpics();
            _client.Verify(c => c.QueryJql("issuetype=Epic"));
        }

        [Fact]
        public async Task GetEpics_should_query_for_epics_honoring_epic_filter()
        {
            _config.EpicQueryFilter = "statusCategory!=Done AND created>-30d";
            await _provider.GetEpics();
            _client.Verify(c => c.QueryJql("issuetype=Epic AND (statusCategory!=Done AND created>-30d)"));
        }

        [Fact]
        public async Task GetEpics_should_query_and_map_epics()
        {
            var elements = new[]
            {
                CreateJsonElement(),
                CreateJsonElement(),
                CreateJsonElement()
            };

            var expectedEpics = elements.Select(SetupEpicMapping).ToArray();
            SetupClient(elements);

            var results = await _provider.GetEpics();
            results.ShouldBe(expectedEpics);
        }

        private Epic SetupEpicMapping(JsonElement element)
        {
            var epic = new Epic();
            _mapper.Setup(x => x.MapEpic(element)).Returns(epic);
            return epic;
        }

        private JsonElement CreateJsonElement() => JsonDocument.Parse($"{{\"id\":\"{Guid.NewGuid()}\"}}").RootElement;

        private void SetupClient(params JsonElement[] epics)
        {
            _client
                .Setup(x => x.QueryJql(It.IsAny<string>()))
                .Returns(Task.FromResult((IReadOnlyList<JsonElement>)epics));
        }
    }
}
