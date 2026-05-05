using System.Text.Json.Serialization;

namespace Skorbord.API.Models
{
    /// <summary>
    /// View için ana veri nesnesi.
    /// UI'ı bozmamak için eski API yapısına benzer tasarlandı.
    /// </summary>
    public class LivescoreViewModel
    {
        public string Date { get; set; } = string.Empty;
        public string FetchedAt { get; set; } = string.Empty;
        public LivescoresSummary Summary { get; set; } = new();
        public List<LeagueViewModel> Leagues { get; set; } = new();
    }

    public class LivescoresSummary
    {
        public int TotalMatches { get; set; }
        public int LiveMatches { get; set; }
        public int FinishedMatches { get; set; }
        public int ScheduledMatches { get; set; }
        public int TotalLeagues { get; set; }
    }

    public class LeagueViewModel
    {
        public string League { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Sport { get; set; } = string.Empty;
        public List<MatchViewModel> Matches { get; set; } = new();
    }

    public class MatchViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public HalfTimeScore? HalfTimeScore { get; set; }
        
        // Örn: "live", "finished", "scheduled" vb.
        public string Status { get; set; } = string.Empty;
        
        // Örn: "post" (maç sonu), "in_progress"
        public string State { get; set; } = string.Empty;
        
        public string StatusCode { get; set; } = string.Empty;
        
        // "14:30" formatında saat
        public string StartTime { get; set; } = string.Empty;
        
        public bool IsLive { get; set; }
        public string MatchUrl { get; set; } = string.Empty;
    }

    public class HalfTimeScore
    {
        public int Home { get; set; }
        public int Away { get; set; }
    }
}
