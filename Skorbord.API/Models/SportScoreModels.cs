using System.Text.Json.Serialization;

namespace Skorbord.API.Models
{
    // API'nin temel dönüş sarmalayıcısı
    public class SportScoreResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;

        [JsonPropertyName("meta")]
        public SportScoreMeta? Meta { get; set; }
    }

    public class SportScoreMeta
    {
        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }
        
        [JsonPropertyName("last_page")]
        public int LastPage { get; set; }
    }

    public class SportScoreEvent
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("status_more")]
        public string StatusMore { get; set; } = string.Empty;

        [JsonPropertyName("start_at")]
        public string StartAt { get; set; } = string.Empty;

        [JsonPropertyName("home_score")]
        public SportScoreScore? HomeScore { get; set; }

        [JsonPropertyName("away_score")]
        public SportScoreScore? AwayScore { get; set; }

        [JsonPropertyName("home_team")]
        public SportScoreTeam HomeTeam { get; set; } = new();

        [JsonPropertyName("away_team")]
        public SportScoreTeam AwayTeam { get; set; } = new();

        [JsonPropertyName("league")]
        public SportScoreLeague League { get; set; } = new();
        
        [JsonPropertyName("sport_id")]
        public int SportId { get; set; }
    }

    public class SportScoreScore
    {
        [JsonPropertyName("current")]
        public int? Current { get; set; }

        [JsonPropertyName("display")]
        public int? Display { get; set; }

        [JsonPropertyName("period_1")]
        public int? Period1 { get; set; }
        
        [JsonPropertyName("period_2")]
        public int? Period2 { get; set; }
    }

    public class SportScoreTeam
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("name_short")]
        public string NameShort { get; set; } = string.Empty;
    }

    public class SportScoreLeague
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("sport_id")]
        public int SportId { get; set; }
    }
}
