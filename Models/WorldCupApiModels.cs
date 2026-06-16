using System.Text.Json.Serialization;

namespace Eval.Models
{
    public class WorldCupData
    {
        [JsonPropertyName("matches")]
        public List<WorldCupMatch> Matches { get; set; } = new();
    }

    public class WorldCupMatch
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("team1")]
        public string Team1 { get; set; } = string.Empty;

        [JsonPropertyName("team2")]
        public string Team2 { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public WorldCupScore? Score { get; set; }
    }

    public class WorldCupScore
    {
        [JsonPropertyName("ft")]
        public List<int>? Ft { get; set; }
    }
}