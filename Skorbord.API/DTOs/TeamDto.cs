namespace Skorbord.API.DTOs
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Stadium { get; set; } = string.Empty;
        public int FoundedYear { get; set; }
        public int LeagueId { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTeamDto
    {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Stadium { get; set; } = string.Empty;
        public int FoundedYear { get; set; }
        public int LeagueId { get; set; }
    }

    public class UpdateTeamDto
    {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Stadium { get; set; } = string.Empty;
        public int FoundedYear { get; set; }
        public int LeagueId { get; set; }
    }
}
