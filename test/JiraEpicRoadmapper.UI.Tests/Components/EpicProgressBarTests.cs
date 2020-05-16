using System.Threading.Tasks;
using Bunit;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.UI.Shared.Components;
using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.Framework.Parameters;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;

namespace JiraEpicRoadmapper.UI.Tests.Components
{
    public class EpicProgressBarTests : FeatureFixture
    {
        [Scenario]
        public async Task Bar_with_no_stats()
        {
            await Runner
                .WithContext<EpicProgressBarFixture>()
                .AddSteps(
                    x => x.Given_a_progress_bar(),
                    x => x.When_I_render_it(),
                    x => x.Then_I_should_see_content("<div class=\"epic-progress-bar loading-bar\" style=\"\">&nbsp;</div>"))
                .RunAsync();
        }

        [Scenario]
        public async Task Bar_with_stats()
        {
            await Runner
                .WithContext<EpicProgressBarFixture>()
                .AddSteps(
                    x => x.Given_a_progress_bar(),
                    x => x.It_should_render_proper_progress_for_data(Table.ExpectData(
                        new Expectation(new EpicStats { }, "background-color: #f0f0f0"),
                        new Expectation(new EpicStats { Done = 1 }, "background:linear-gradient(to right, #50ff50 0% 100%);"),
                        new Expectation(new EpicStats { InProgress = 1 }, "background:linear-gradient(to right, #5050ff 0% 100%);"),
                        new Expectation(new EpicStats { NotStarted = 1 }, "background:linear-gradient(to right, #505050 0% 100%);"),
                        new Expectation(new EpicStats { Done = 1, InProgress = 1 }, "background:linear-gradient(to right, #50ff50 0% 50%, #5050ff 50% 100%);"),
                        new Expectation(new EpicStats { Done = 1, NotStarted = 1 }, "background:linear-gradient(to right, #50ff50 0% 50%, #505050 50% 100%);"),
                        new Expectation(new EpicStats { InProgress = 1, NotStarted = 1 }, "background:linear-gradient(to right, #5050ff 0% 50%, #505050 50% 100%);"),
                        new Expectation(new EpicStats { Done = 1, InProgress = 1, NotStarted = 1 }, "background:linear-gradient(to right, #50ff50 0% 33%, #5050ff 33% 66%, #505050 66% 100%);"),
                        new Expectation(new EpicStats { Done = 1, InProgress = 2, NotStarted = 1 }, "background:linear-gradient(to right, #50ff50 0% 25%, #5050ff 25% 75%, #505050 75% 100%);"),
                        new Expectation(new EpicStats { Done = 2, InProgress = 1, NotStarted = 1 }, "background:linear-gradient(to right, #50ff50 0% 50%, #5050ff 50% 75%, #505050 75% 100%);"),
                        new Expectation(new EpicStats { Done = 1, InProgress = 1, NotStarted = 2 }, "background:linear-gradient(to right, #50ff50 0% 25%, #5050ff 25% 50%, #505050 50% 100%);")
                        )))
                .RunAsync();
        }

        public class Expectation
        {
            public EpicStats Input { get; }
            public string Content { get; }

            public Expectation(EpicStats input, string content)
            {
                Input = input;
                Content = content;
            }
        }

        public class EpicProgressBarFixture : ComponentFixture<EpicProgressBar>
        {
            public void Given_a_progress_bar()
            {
            }

            public void It_should_render_proper_progress_for_data(VerifiableDataTable<Expectation> data)
            {
                data.SetActual(e =>
                {
                    WithComponentParameter(ComponentParameter.CreateParameter(nameof(EpicProgressBar.Stats), e.Input));
                    var style = RenderedComponentWithParameters().Find("div.epic-progress-bar").Attributes.GetNamedItem("style").Value;
                    return new Expectation(e.Input, style);
                });
            }
        }
    }
}