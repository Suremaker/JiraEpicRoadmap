using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Shared;

namespace JiraEpicRoadmapper.Client
{
    public static class Extensions
    {
        public static DateTimeOffset GetFirstOfMonth(this DateTimeOffset x) => x.AddDays(-x.Day+1);
    }
}
