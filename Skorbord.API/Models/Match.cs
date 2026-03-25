namespace Skorbord.API.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public int LeagueId { get; set; }
        public DateTime MatchDate { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, Live, Finished, Postponed
        public int HomeScore { get; set; } = 0;
        public int AwayScore { get; set; } = 0;
        public string Venue { get; set; } = string.Empty;
        public int Round { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public virtual League League { get; set; } = null!;
        public virtual Team HomeTeam { get; set; } = null!;
        public virtual Team AwayTeam { get; set; } = null!;
    }
}
