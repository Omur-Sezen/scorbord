namespace Skorbord.API.DTOs
{
    public class MatchDto
    {
        public int Id { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public int LeagueId { get; set; }
        public DateTime MatchDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string Venue { get; set; } = string.Empty;
        public int Round { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateMatchDto
    {
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public int LeagueId { get; set; }
        public DateTime MatchDate { get; set; }
        public string Venue { get; set; } = string.Empty;
        public int Round { get; set; }
    }

    public class UpdateMatchDto
    {
        public string Status { get; set; } = string.Empty;
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public DateTime MatchDate { get; set; }
        public string Venue { get; set; } = string.Empty;
    }
}
