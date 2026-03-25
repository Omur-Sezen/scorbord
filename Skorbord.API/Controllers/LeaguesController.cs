using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skorbord.API.Data;
using Skorbord.API.DTOs;
using Skorbord.API.Models;

namespace Skorbord.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaguesController : ControllerBase
    {
        private readonly SkorbordDbContext _context;

        public LeaguesController(SkorbordDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeagueDto>>> GetLeagues()
        {
            var leagues = await _context.Leagues
                .Select(l => new LeagueDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Country = l.Country,
                    Season = l.Season,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return Ok(leagues);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeagueDto>> GetLeague(int id)
        {
            var league = await _context.Leagues
                .Where(l => l.Id == id)
                .Select(l => new LeagueDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Country = l.Country,
                    Season = l.Season,
                    CreatedAt = l.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (league == null)
            {
                return NotFound();
            }

            return Ok(league);
        }

        [HttpPost]
        public async Task<ActionResult<LeagueDto>> CreateLeague(CreateLeagueDto createLeagueDto)
        {
            var league = new League
            {
                Name = createLeagueDto.Name,
                Country = createLeagueDto.Country,
                Season = createLeagueDto.Season,
                CreatedAt = DateTime.UtcNow
            };

            _context.Leagues.Add(league);
            await _context.SaveChangesAsync();

            var leagueDto = new LeagueDto
            {
                Id = league.Id,
                Name = league.Name,
                Country = league.Country,
                Season = league.Season,
                CreatedAt = league.CreatedAt
            };

            return CreatedAtAction(nameof(GetLeague), new { id = league.Id }, leagueDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLeague(int id, UpdateLeagueDto updateLeagueDto)
        {
            var league = await _context.Leagues.FindAsync(id);
            if (league == null)
            {
                return NotFound();
            }

            league.Name = updateLeagueDto.Name;
            league.Country = updateLeagueDto.Country;
            league.Season = updateLeagueDto.Season;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeague(int id)
        {
            var league = await _context.Leagues.FindAsync(id);
            if (league == null)
            {
                return NotFound();
            }

            _context.Leagues.Remove(league);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
