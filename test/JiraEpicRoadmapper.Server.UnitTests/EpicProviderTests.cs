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
        public async Task GetEpics_should_query_for_fields_once()
        {
            _client.Setup(x => x.QueryFieldNameToKeysMap()).ReturnsAsync(new Dictionary<string, string[]>());

            await _provider.GetEpics();
            await _provider.GetEpics();

            _client.Verify(c => c.QueryFieldNameToKeysMap(), Times.Once);
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

        [Fact]
        public async Task GetEpics_should_map_epics_using_fetched_fields_map()
        {
            var element = CreateJsonElement();
            SetupClient(element);

            var fieldsMap = new Dictionary<string, string[]>();
            _client.Setup(x => x.QueryFieldNameToKeysMap()).ReturnsAsync(fieldsMap);

            var _ = (await _provider.GetEpics()).ToArray();

            _mapper.Verify(x => x.MapEpic(element, fieldsMap));
        }

        [Fact]
        public async Task GetEpicStats_should_query_for_tickets_belonging_to_the_epic()
        {
            await _provider.GetEpicStats("foo");
            _client.Verify(x => x.QueryJql("\"Epic Link\"=foo"));
        }

        [Fact]
        public async Task GetEpicStats_should_aggregate_tickets_on_status_category()
        {
            SetupClient(
                CreateTicketWithStatusCategoryJsonElement("done"),
                CreateTicketWithStatusCategoryJsonElement("done"),
                CreateTicketWithStatusCategoryJsonElement("done"),
                CreateTicketWithStatusCategoryJsonElement("in progress"),
                CreateTicketWithStatusCategoryJsonElement("in progress"),
                CreateTicketWithStatusCategoryJsonElement("to do")
                );
            var result = await _provider.GetEpicStats("foo");
            result.Done.ShouldBe(3);
            result.InProgress.ShouldBe(2);
            result.NotStarted.ShouldBe(1);
            result.Total.ShouldBe(6);
        }

        [Fact]
        public async Task UpdateEpics_should_update_epic_and_return_its_new_content()
        {
            var dueValue = "2020-01-15";
            var startValue = "2020-01-02";
            var epicKey = "KEY-11";
            var epicJson = CreateJsonElement();
            var updatedEpic = new Epic();
            IReadOnlyDictionary<string, string[]> fields = new Dictionary<string, string[]> { { "Start date", new[] { "custom_1" } } };
            _client.Setup(x => x.QueryFieldNameToKeysMap()).ReturnsAsync(fields);
            _client.Setup(x => x.UpdateIssue(epicKey, It.IsAny<IssueContent>())).Returns(Task.CompletedTask);
            _client.Setup(x => x.QueryJql("key=KEY-11")).ReturnsAsync(new[] { epicJson });
            _mapper.Setup(x => x.MapEpic(epicJson, fields)).Returns(updatedEpic);
            var epic = await _provider.UpdateEpic(epicKey, new EpicMeta { DueDate = DateTimeOffset.Parse(dueValue), StartDate = DateTimeOffset.Parse(startValue) });

            epic.ShouldBeSameAs(updatedEpic);

            var expectedFields = new Dictionary<string, string> { { "custom_1", startValue }, { "duedate", dueValue } };
            _client.Verify(x => x.UpdateIssue(epicKey, It.Is<IssueContent>(c => AreFieldsEqual(c, expectedFields))));
        }

        private static bool AreFieldsEqual(IssueContent c, IDictionary<string, string> expectedFields)
        {
            c.Fields.ShouldBe(expectedFields, ignoreOrder: true);
            return true;
        }

        private Epic SetupEpicMapping(JsonElement element)
        {
            var epic = new Epic();
            _mapper.Setup(x => x.MapEpic(element, It.IsAny<IReadOnlyDictionary<string, string[]>>())).Returns(epic);
            return epic;
        }

        private JsonElement CreateJsonElement() => JsonDocument.Parse($"{{\"id\":\"{Guid.NewGuid()}\"}}").RootElement;

        private JsonElement CreateTicketWithStatusCategoryJsonElement(string statusCategory) => JsonDocument.Parse($@"{{
    ""fields"": {{
        ""status"": {{
            ""statusCategory"": {{
                ""name"": ""{statusCategory}""
            }}
        }}
    }}
}}").RootElement;


        private void SetupClient(params JsonElement[] epics)
        {
            _client
                .Setup(x => x.QueryJql(It.IsAny<string>()))
                .Returns(Task.FromResult((IReadOnlyList<JsonElement>)epics));
        }
    }
}
