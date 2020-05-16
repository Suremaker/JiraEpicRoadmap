using System;
using System.Threading.Tasks;
using Bunit;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Models;
using JiraEpicRoadmapper.UI.Repositories;
using JiraEpicRoadmapper.UI.Shared;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class EpicDetailsViewTests : FeatureFixture
    {
        [Scenario]
        public async Task Epic_card_content()
        {
            await Runner
                .WithContext<EpicDetailsViewFixture>()
                .AddSteps(
                    x => x.Given_a_epic_details_view(),
                    x => x.Given_epic_has_summary("Hello world"),
                    x => x.Given_epic_has_status("in progress"),
                    x => x.Given_epic_has_key("EP-11"),
                    x => x.Given_epic_has_url("http://a/EP-11"),
                    x => x.Given_epic_has_start_date("2020-03-04"),
                    x => x.Given_epic_has_due_date("2020-03-14"),
                    x => x.Given_epic_has_stats(new EpicStats { Done = 1, InProgress = 2, NotStarted = 3 }),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_the_rendered_table("<table>\r\n  <tbody>\r\n    <tr>\r\n      <td>Project</td>\r\n      <td></td>\r\n    </tr>\r\n    <tr>\r\n      <td>Key</td>\r\n      <td>\r\n        <a href=\"http://a/EP-11\" target=\"_blank\">EP-11</a>\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td>Summary</td>\r\n      <td>\r\n        <a href=\"http://a/EP-11\" target=\"_blank\">Hello world</a>\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td>Status</td>\r\n      <td>in progress</td>\r\n    </tr>\r\n    <tr>\r\n      <td>Start</td>\r\n      <td>\r\n        <input type=\"date\" value=\"2020-03-04\">\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td>Due</td>\r\n      <td>\r\n        <input type=\"date\" value=\"2020-03-14\">\r\n      </td>\r\n    </tr>\r\n    <tr>\r\n      <td>Tickets</td>\r\n      <td>\r\n        <div>1 done</div>\r\n        <div>2 in progress</div>\r\n        <div>3 not started</div>\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n")
                    )
                .RunAsync();
        }

        //TODO: test updating epic

        public class EpicDetailsViewFixture : ComponentFixture<EpicDetailsView>
        {
            public EpicCard Card { get; } = new EpicCard(new EpicMetadata(new Epic(), new IndexedDay(DateTimeOffset.MinValue, 3), new IndexedDay(DateTimeOffset.MinValue, 5)), 1);
            public EpicMetadata Meta => Card.Meta;
            public Epic Epic => Meta.Epic;

            public void Given_a_epic_details_view()
            {
                Services.AddSingleton(Mock.Of<IEpicsRepository>());
                WithComponentParameter(ComponentParameter.CreateParameter(nameof(EpicDetailsView.Card), Card));
            }

            public void Given_epic_has_summary(string summary)
            {
                Epic.Summary = summary;
            }

            public void Given_epic_has_status(string status)
            {
                Epic.Status = status;
            }

            public void Given_epic_has_key(string key)
            {
                Epic.Key = key;
            }

            public void Given_epic_has_url(string url)
            {
                Epic.Url = url;
            }

            public void Given_epic_has_start_date(string date)
            {
                Epic.StartDate = DateTimeOffset.Parse(date);
            }

            public void Given_epic_has_due_date(string date)
            {
                Epic.DueDate = DateTimeOffset.Parse(date);
            }

            public void Given_epic_has_stats(EpicStats stats)
            {
                Meta.Stats = stats;
            }

            public void Then_I_should_see_the_rendered_table(string table)
            {
                Component.Find("table").MarkupMatches(table);
            }
        }
    }
}