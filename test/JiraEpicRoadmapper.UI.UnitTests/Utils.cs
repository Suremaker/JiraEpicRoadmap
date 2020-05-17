using System;

namespace JiraEpicRoadmapper.UI.UnitTests
{
    static class Utils
    {
        public static DateTime? ToNullableDateTime(string date)
        {
            return DateTime.TryParse(date, out var result)
                ? (DateTime?)result
                : null;
        }
    }
}