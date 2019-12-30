using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraEpicVisualizer
{
    static class Extensions
    {
        public static async Task<JsonDocument> Query(this HttpClient client, string query)
        {
            using var resp = await client.GetAsync(query);
            return await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
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
