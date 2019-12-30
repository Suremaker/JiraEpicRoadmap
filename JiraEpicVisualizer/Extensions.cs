using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraEpicVisualizer
{
    static class Extensions
    {
        public static async Task<IReadOnlyList<JsonElement>> Query(this HttpClient client, string query)
        {
            query = Uri.EscapeDataString(query);
            var elements = new List<JsonElement>();
            int start = 0;
            while (true)
            {
                using var resp = await client.GetAsync($"/rest/api/2/search?jql={query}&startAt={start}&maxResults=50");
                var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
                var issues = doc.RootElement.GetProperty("issues");

                foreach (var e in issues.EnumerateArray())
                {
                    elements.Add(e);
                }
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
    }
}
