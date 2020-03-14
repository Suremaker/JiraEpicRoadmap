using System;

namespace JiraEpicRoadmapper.Client
{
    public static class Extensions
    {
        public static DateTimeOffset GetFirstOfMonth(this DateTimeOffset x) => x.AddDays(-x.Day + 1);
    }
}
