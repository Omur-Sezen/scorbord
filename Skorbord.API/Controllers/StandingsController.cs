using Microsoft.AspNetCore.Mvc;
using Skorbord.API.Services;
using Skorbord.API.DTOs;

namespace Skorbord.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StandingsController : ControllerBase
    {
        private readonly StandingsService _standingsService;

        public StandingsController(StandingsService standingsService)
        {
            _standingsService = standingsService;
        }

        [HttpGet("league/{leagueId}")]
        public async Task<ActionResult<LeagueStandingsDto>> GetLeagueStandings(int leagueId)
        {
            try
            {
                var standings = await _standingsService.GetLeagueStandingsAsync(leagueId);
                return Ok(standings);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<LeagueStandingsDto>>> GetAllStandings()
        {
            var allStandings = await _standingsService.GetAllLeaguesStandingsAsync();
            return Ok(allStandings);
        }
    }
}
