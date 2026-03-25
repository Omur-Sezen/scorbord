using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skorbord.API.Data;
using Skorbord.API.DTOs;
using Skorbord.API.Models;

namespace Skorbord.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly SkorbordDbContext _context;

        public MatchesController(SkorbordDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches()
        {
            var matches = await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.League)
                .Select(m => new MatchDto
                {
                    Id = m.Id,
                    HomeTeamName = m.HomeTeam.Name,
                    AwayTeamName = m.AwayTeam.Name,
                    HomeTeamId = m.HomeTeamId,
                    AwayTeamId = m.AwayTeamId,
                    LeagueName = m.League.Name,
                    LeagueId = m.LeagueId,
                    MatchDate = m.MatchDate,
                    Status = m.Status,
                    HomeScore = m.HomeScore,
                    AwayScore = m.AwayScore,
                    Venue = m.Venue,
                    Round = m.Round,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return Ok(matches);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MatchDto>> GetMatch(int id)
        {
            var match = await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.League)
                .Where(m => m.Id == id)
                .Select(m => new MatchDto
                {
                    Id = m.Id,
                    HomeTeamName = m.HomeTeam.Name,
                    AwayTeamName = m.AwayTeam.Name,
                    HomeTeamId = m.HomeTeamId,
                    AwayTeamId = m.AwayTeamId,
                    LeagueName = m.League.Name,
                    LeagueId = m.LeagueId,
                    MatchDate = m.MatchDate,
                    Status = m.Status,
                    HomeScore = m.HomeScore,
                    AwayScore = m.AwayScore,
                    Venue = m.Venue,
                    Round = m.Round,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (match == null)
            {
                return NotFound();
            }

            return Ok(match);
        }

        [HttpGet("league/{leagueId}")]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatchesByLeague(int leagueId)
        {
            var matches = await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.League)
                .Where(m => m.LeagueId == leagueId)
                .OrderBy(m => m.MatchDate)
                .Select(m => new MatchDto
                {
                    Id = m.Id,
                    HomeTeamName = m.HomeTeam.Name,
                    AwayTeamName = m.AwayTeam.Name,
                    HomeTeamId = m.HomeTeamId,
                    AwayTeamId = m.AwayTeamId,
                    LeagueName = m.League.Name,
                    LeagueId = m.LeagueId,
                    MatchDate = m.MatchDate,
                    Status = m.Status,
                    HomeScore = m.HomeScore,
                    AwayScore = m.AwayScore,
                    Venue = m.Venue,
                    Round = m.Round,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return Ok(matches);
        }

        [HttpGet("team/{teamId}")]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatchesByTeam(int teamId)
        {
            var matches = await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.League)
                .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                .OrderBy(m => m.MatchDate)
                .Select(m => new MatchDto
                {
                    Id = m.Id,
                    HomeTeamName = m.HomeTeam.Name,
                    AwayTeamName = m.AwayTeam.Name,
                    HomeTeamId = m.HomeTeamId,
                    AwayTeamId = m.AwayTeamId,
                    LeagueName = m.League.Name,
                    LeagueId = m.LeagueId,
                    MatchDate = m.MatchDate,
                    Status = m.Status,
                    HomeScore = m.HomeScore,
                    AwayScore = m.AwayScore,
                    Venue = m.Venue,
                    Round = m.Round,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return Ok(matches);
        }

        [HttpPost]
        public async Task<ActionResult<MatchDto>> CreateMatch(CreateMatchDto createMatchDto)
        {
            var match = new Match
            {
                HomeTeamId = createMatchDto.HomeTeamId,
                AwayTeamId = createMatchDto.AwayTeamId,
                LeagueId = createMatchDto.LeagueId,
                MatchDate = createMatchDto.MatchDate,
                Venue = createMatchDto.Venue,
                Round = createMatchDto.Round,
                Status = "Scheduled",
                HomeScore = 0,
                AwayScore = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            var homeTeam = await _context.Teams.FindAsync(match.HomeTeamId);
            var awayTeam = await _context.Teams.FindAsync(match.AwayTeamId);
            var league = await _context.Leagues.FindAsync(match.LeagueId);

            var matchDto = new MatchDto
            {
                Id = match.Id,
                HomeTeamName = homeTeam?.Name ?? "",
                AwayTeamName = awayTeam?.Name ?? "",
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                LeagueName = league?.Name ?? "",
                LeagueId = match.LeagueId,
                MatchDate = match.MatchDate,
                Status = match.Status,
                HomeScore = match.HomeScore,
                AwayScore = match.AwayScore,
                Venue = match.Venue,
                Round = match.Round,
                CreatedAt = match.CreatedAt,
                UpdatedAt = match.UpdatedAt
            };

            return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, matchDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMatch(int id, UpdateMatchDto updateMatchDto)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            match.Status = updateMatchDto.Status;
            match.HomeScore = updateMatchDto.HomeScore;
            match.AwayScore = updateMatchDto.AwayScore;
            match.MatchDate = updateMatchDto.MatchDate;
            match.Venue = updateMatchDto.Venue;
            match.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
