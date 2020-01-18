using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraEpicVisualizer
{
    public static class Extensions
    {
        public static async Task<IReadOnlyList<JsonElement>> Query(this HttpClient client, string query)
        {
            query = Uri.EscapeDataString(query);
            var elements = new List<JsonElement>();
            var start = 0;
            while (true)
            {
                using var resp = await client.GetAsync($"/rest/api/2/search?jql={query}&startAt={start}&maxResults=50");
                var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
                var issues = doc.RootElement.GetProperty("issues");

                elements.AddRange(issues.EnumerateArray());
                start = doc.RootElement.GetProperty("startAt").GetInt32();
                var total = doc.RootElement.GetProperty("total").GetInt32();
                var maxResults = doc.RootElement.GetProperty("maxResults").GetInt32();
                start += maxResults;
                if (start >= total)
                    break;
            }
            return elements;
        }

        public static string ToJsonString(this JsonDocument jdoc)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                jdoc.WriteTo(writer);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static string ToPlainText(this string txt) => txt.Replace(",", "").Replace("\"", "").Replace("'", "").Replace("`", "");

        public static Color GetSeverityColor(this Epic e)
        {
            var ranges = new[]
            {
                new {value = int.MaxValue, color = Color.FromArgb(0xaaaaaa)},
                new {value = 90, color = Color.FromArgb(0xaaaaaa)},
                new {value = 60, color = Color.FromArgb(0xaaffaa)},
                new {value = 30, color = Color.FromArgb(0xffffaa)},
                new {value = 15, color = Color.FromArgb(0xffff30)},
                new {value = 0, color = Color.FromArgb(0xff3030)},
                new {value = int.MinValue, color = Color.FromArgb(0xff3030)}
            };

            var span = e.DueDate.GetValueOrDefault(DateTimeOffset.MaxValue).Date - DateTimeOffset.UtcNow.Date;

            var i = 0;
            var days = span.Days;
            while (ranges[i + 1].value > days) i++;
            var a = ranges[i];
            var b = ranges[i + 1];
            var progress = (a.value - days) / (double)(a.value - b.value);
            return MorphTo(a.color, b.color, progress);
        }

        public static Color MorphTo(this Color from, Color to,double progress)
        {
            return Color.FromArgb(
                (int)(from.R - (progress * (from.R - to.R))),
                (int)(from.G - (progress * (from.G - to.G))),
                (int)(from.B - (progress * (from.B - to.B)))
            );
        }

        public static Color ToBorder(this Color color) => Color.FromArgb((int)(color.R * 0.6), (int)(color.G * 0.6), (int)(color.B * 0.6));
        public static string ToRgb(this Color color) => $"#{color.R:x2}{color.G:x2}{color.B:x2}";
    }
}
