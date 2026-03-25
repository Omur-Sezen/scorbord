using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skorbord.API.Data;
using Skorbord.API.DTOs;
using Skorbord.API.Models;

namespace Skorbord.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly SkorbordDbContext _context;

        public TeamsController(SkorbordDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams()
        {
            var teams = await _context.Teams
                .Include(t => t.League)
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ShortName = t.ShortName,
                    LogoUrl = t.LogoUrl,
                    Stadium = t.Stadium,
                    FoundedYear = t.FoundedYear,
                    LeagueId = t.LeagueId,
                    LeagueName = t.League.Name,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(teams);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeamDto>> GetTeam(int id)
        {
            var team = await _context.Teams
                .Include(t => t.League)
                .Where(t => t.Id == id)
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ShortName = t.ShortName,
                    LogoUrl = t.LogoUrl,
                    Stadium = t.Stadium,
                    FoundedYear = t.FoundedYear,
                    LeagueId = t.LeagueId,
                    LeagueName = t.League.Name,
                    CreatedAt = t.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (team == null)
            {
                return NotFound();
            }

            return Ok(team);
        }

        [HttpGet("league/{leagueId}")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeamsByLeague(int leagueId)
        {
            var teams = await _context.Teams
                .Include(t => t.League)
                .Where(t => t.LeagueId == leagueId)
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ShortName = t.ShortName,
                    LogoUrl = t.LogoUrl,
                    Stadium = t.Stadium,
                    FoundedYear = t.FoundedYear,
                    LeagueId = t.LeagueId,
                    LeagueName = t.League.Name,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(teams);
        }

        [HttpPost]
        public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto createTeamDto)
        {
            var team = new Team
            {
                Name = createTeamDto.Name,
                ShortName = createTeamDto.ShortName,
                LogoUrl = createTeamDto.LogoUrl,
                Stadium = createTeamDto.Stadium,
                FoundedYear = createTeamDto.FoundedYear,
                LeagueId = createTeamDto.LeagueId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            var teamDto = new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                ShortName = team.ShortName,
                LogoUrl = team.LogoUrl,
                Stadium = team.Stadium,
                FoundedYear = team.FoundedYear,
                LeagueId = team.LeagueId,
                LeagueName = (await _context.Leagues.FindAsync(team.LeagueId))?.Name ?? "",
                CreatedAt = team.CreatedAt
            };

            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, teamDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(int id, UpdateTeamDto updateTeamDto)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            team.Name = updateTeamDto.Name;
            team.ShortName = updateTeamDto.ShortName;
            team.LogoUrl = updateTeamDto.LogoUrl;
            team.Stadium = updateTeamDto.Stadium;
            team.FoundedYear = updateTeamDto.FoundedYear;
            team.LeagueId = updateTeamDto.LeagueId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
