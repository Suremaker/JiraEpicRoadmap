using System;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    static class Utils
    {
        public static DateTimeOffset? ToNullableDateTime(string date)
        {
            return DateTimeOffset.TryParse(date, out var result)
                ? (DateTimeOffset?)result
                : null;
        }
    }
}