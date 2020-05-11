using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JiraEpicRoadmapper.UI.Models
{
    public class EpicCardPainter : IEpicCardPainter
    {
        private byte MinColorValue = 100;
        private readonly ConcurrentDictionary<string, string> _codes = new ConcurrentDictionary<string, string>();

        public string GetColor(EpicVisualBlock block) => _codes.GetOrAdd(block.Meta.Epic.Project ?? string.Empty, GenerateColor);

        private string GenerateColor(string key)
        {
            using (var md5 = MD5.Create())
            {
                var rgb = md5.ComputeHash(Encoding.Default.GetBytes(key)).Take(3).ToArray();
                return ToHtmlColor(ScaleColor(rgb));
            }
        }

        private string ToHtmlColor(byte[] rgb) => $"#{rgb[0]:x2}{rgb[1]:x2}{rgb[2]:x2}";

        private byte[] ScaleColor(byte[] rgb)
        {
            var min = rgb.Min();
            var mul = (byte.MaxValue - MinColorValue) / (float)(byte.MaxValue - min);

            for (int i = 0; i < rgb.Length; ++i)
                rgb[i] = (byte)((rgb[i] - min) * mul + MinColorValue);

            return rgb;
        }
    }
}