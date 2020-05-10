using JiraEpicRoadmapper.UI.Tests.Scaffolding;
using LightBDD.XUnit2;

[assembly: ConfiguredLightBddScope]
[assembly: ClassCollectionBehavior(AllowTestParallelization = true)]

namespace JiraEpicRoadmapper.UI.Tests.Scaffolding
{
    class ConfiguredLightBddScope : LightBddScopeAttribute
    {
    }
}
