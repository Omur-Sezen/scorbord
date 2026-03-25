using Microsoft.EntityFrameworkCore;
using Skorbord.API.Data;
using Skorbord.API.DTOs;
using Skorbord.API.Models;

namespace Skorbord.API.Services
{
    public class StandingsService
    {
        private readonly SkorbordDbContext _context;

        public StandingsService(SkorbordDbContext context)
        {
            _context = context;
        }

        public async Task<LeagueStandingsDto> GetLeagueStandingsAsync(int leagueId)
        {
            var league = await _context.Leagues.FindAsync(leagueId);
            if (league == null)
            {
                throw new ArgumentException("League not found");
            }

            var teams = await _context.Teams
                .Where(t => t.LeagueId == leagueId)
                .ToListAsync();

            var standings = new List<StandingsDto>();

            foreach (var team in teams)
            {
                standings.Add(new StandingsDto
                {
                    Position = team.Position,
                    TeamId = team.Id,
                    TeamName = team.Name,
                    TeamShortName = team.ShortName,
                    Played = team.Played,
                    Won = team.Won,
                    Drawn = team.Drawn,
                    Lost = team.Lost,
                    GoalsFor = team.GoalsFor,
                    GoalsAgainst = team.GoalsAgainst,
                    GoalDifference = team.GoalDifference,
                    Points = team.Points
                });
            }

            // Order correctly based on API positions
            var sortedStandings = standings
                .OrderBy(s => s.Position > 0 ? s.Position : int.MaxValue)
                .ThenByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenBy(s => s.TeamName)
                .ToList();

            return new LeagueStandingsDto
            {
                LeagueId = league.Id,
                LeagueName = league.Name,
                Season = league.Season,
                Standings = sortedStandings
            };
        }

        public async Task<List<LeagueStandingsDto>> GetAllLeaguesStandingsAsync()
        {
            var leagues = await _context.Leagues.ToListAsync();
            var allStandings = new List<LeagueStandingsDto>();

            foreach (var league in leagues)
            {
                try
                {
                    var standings = await GetLeagueStandingsAsync(league.Id);
                    allStandings.Add(standings);
                }
                catch
                {
                    // Skip leagues that don't have teams or matches
                    continue;
                }
            }

            return allStandings;
        }
    }
}
