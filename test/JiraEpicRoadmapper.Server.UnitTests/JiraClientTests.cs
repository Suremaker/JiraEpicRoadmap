using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Server.Clients;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace JiraEpicRoadmapper.Server.UnitTests
{
    public class JiraClientTests
    {
        private readonly Mock<IMockableMessageHandler> _handler = new Mock<IMockableMessageHandler>(MockBehavior.Strict);
        private readonly JiraClient _client;
        private const string BaseUri = "http://foo.bar";

        public JiraClientTests()
        {
            var factory = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient(new MockableMessageHandler(_handler.Object)) { BaseAddress = new Uri(BaseUri) };
            factory.Setup(x => x.CreateClient(nameof(IJiraClient))).Returns(httpClient);
            _client = new JiraClient(new OptionsWrapper<Config>(new Config()), factory.Object);
        }

        [Fact]
        public async Task QueryJql_should_specify_batches()
        {
            SetupJqlBatches();
            var jqlQuery = "project=foo";
            await _client.QueryJql(jqlQuery);
            _handler.Verify(h => h.OnSendAsync(It.Is<HttpRequestMessage>(m => m.RequestUri.ToString().Equals($"{BaseUri}/rest/api/2/search?jql={Uri.EscapeDataString(jqlQuery)}&startAt=0&maxResults=50"))));
        }

        [Fact]
        public async Task QueryJql_should_combine_batched_response()
        {
            var guids = SetupJqlBatches(200);
            var items = await _client.QueryJql("project=bar");

            items.Select(i => i.GetProperty("id").GetGuid()).ShouldBe(guids);
        }

        private class MockableMessageHandler : HttpMessageHandler
        {
            private readonly IMockableMessageHandler _handler;
            public MockableMessageHandler(IMockableMessageHandler handler) => _handler = handler;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _handler.OnSendAsync(request);
        }

        public interface IMockableMessageHandler
        {
            Task<HttpResponseMessage> OnSendAsync(HttpRequestMessage request);
        }

        private Guid[] SetupJqlBatches(int total = 0)
        {
            var guids = Enumerable.Range(0, total).Select(x => Guid.NewGuid()).ToArray();
            for (int i = 0; i <= total; i += 50)
                SetupJqlResponse(i, 50, total, guids.Skip(i).Take(50).Select(g => (object)new { id = g }).ToArray());

            return guids;
        }
        private void SetupJqlResponse(int startAt = 0, int maxResults = 50, int total = 0, params object[] issues)
        {
            var json = new
            {
                startAt = startAt,
                maxResults = maxResults,
                total = total,
                issues = issues
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(json)) };

            _handler
                .Setup(x => x.OnSendAsync(It.Is<HttpRequestMessage>(m => m.RequestUri.Query.Contains($"startAt={startAt}&"))))
                .ReturnsAsync(response);
        }
    }
}
