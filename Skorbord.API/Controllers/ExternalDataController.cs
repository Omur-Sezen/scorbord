using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skorbord.API.Data;
using Skorbord.API.Services;

namespace Skorbord.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalDataController : ControllerBase
    {
        private readonly ExternalApiService _externalApiService;
        private readonly DataProcessingService _dataProcessingService;
        private readonly SkorbordDbContext _context;
        private readonly ILogger<ExternalDataController> _logger;

        public ExternalDataController(ExternalApiService externalApiService, DataProcessingService dataProcessingService, SkorbordDbContext context, ILogger<ExternalDataController> logger)
        {
            _externalApiService = externalApiService;
            _dataProcessingService = dataProcessingService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("sync/leagues")]
        public async Task<IActionResult> SyncLeagues()
        {
            try
            {
                var leaguesData = await _externalApiService.GetCompetitionsAsync();
                if (string.IsNullOrEmpty(leaguesData))
                {
                    return BadRequest("Failed to retrieve leagues data from external API");
                }

                await _dataProcessingService.ProcessAndSaveLeaguesAsync(leaguesData);
                
                return Ok(new { Message = "Leagues synchronized successfully", Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error synchronizing leagues: {ex.Message}");
            }
        }

        [HttpPost("sync/teams/{leagueId}")]
        public async Task<IActionResult> SyncTeams(int leagueId, int season = 2023)
        {
            try
            {
                var teamsData = await _externalApiService.GetTeamsByLeagueIdAsync(leagueId, season);
                if (string.IsNullOrEmpty(teamsData))
                {
                    return BadRequest($"Failed to retrieve teams data for league {leagueId}, season {season}");
                }

                await _dataProcessingService.ProcessAndSaveTeamsAsync(teamsData, leagueId);
                
                return Ok(new { Message = $"Teams synchronized successfully for league {leagueId}", Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error synchronizing teams: {ex.Message}");
            }
        }

        [HttpPost("sync/matches/{leagueId}")]
        public async Task<IActionResult> SyncMatches(int leagueId, int season = 2023)
        {
            try
            {
                var matchesData = await _externalApiService.GetFixturesByLeagueIdAsync(leagueId, season);
                if (string.IsNullOrEmpty(matchesData))
                {
                    return BadRequest($"Failed to retrieve matches data for league {leagueId}, season {season}");
                }

                await _dataProcessingService.ProcessAndSaveMatchesAsync(matchesData, leagueId);
                
                return Ok(new { Message = $"Matches synchronized successfully for league {leagueId}", Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error synchronizing matches: {ex.Message}");
            }
        }

        [HttpPost("sync/full/{leagueId}")]
        public async Task<IActionResult> FullSync(int leagueId, int season = 2023)
        {
            try
            {
                // First, check if league exists
                var league = await _context.Leagues.FindAsync(leagueId);
                if (league == null)
                {
                    return BadRequest($"League with ID {leagueId} not found. Please sync leagues first.");
                }

                // Sync teams first
                var teamsData = await _externalApiService.GetTeamsByLeagueIdAsync(leagueId, season);
                if (!string.IsNullOrEmpty(teamsData))
                {
                    await _dataProcessingService.ProcessAndSaveTeamsAsync(teamsData, leagueId);
                    _logger.LogInformation($"Teams synced for league {leagueId}");
                }
                else
                {
                    _logger.LogWarning($"No teams data received for league {leagueId}. Teams endpoint may require paid subscription.");
                }

                // Check if any teams were saved
                var teamCount = await _context.Teams.CountAsync(t => t.LeagueId == leagueId);
                if (teamCount == 0)
                {
                    return BadRequest($"No teams found for league {leagueId}. Cannot sync matches without teams.");
                }

                // Then sync matches
                var matchesData = await _externalApiService.GetFixturesByLeagueIdAsync(leagueId, season);
                if (!string.IsNullOrEmpty(matchesData))
                {
                    await _dataProcessingService.ProcessAndSaveMatchesAsync(matchesData, leagueId);
                    _logger.LogInformation($"Matches synced for league {leagueId}");
                }
                else
                {
                    _logger.LogWarning($"No matches data received for league {leagueId}");
                }
                
                return Ok(new { 
                    Message = $"Full synchronization completed for league {leagueId}", 
                    TeamsCount = teamCount,
                    Timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during full synchronization for league {leagueId}");
                return StatusCode(500, $"Error during full synchronization: {ex.Message}. Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpPost("sync/livescores")]
        public async Task<IActionResult> SyncLiveScores()
        {
            try
            {
                // Get live scores from Sportmonks
                var liveScoresData = await _externalApiService.GetLiveScoresAsync();
                if (string.IsNullOrEmpty(liveScoresData))
                {
                    return BadRequest("No live scores data received from Sportmonks API");
                }

                // Process and save live scores
                await _dataProcessingService.ProcessAndSaveLiveScoresAsync(liveScoresData);
                
                return Ok(new { 
                    Message = "Live scores synchronized successfully", 
                    Timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing live scores");
                return StatusCode(500, $"Error synchronizing live scores: {ex.Message}");
            }
        }

        [HttpPost("sync/livescores/inplay")]
        public async Task<IActionResult> SyncInplayLivescores()
        {
            try
            {
                // Get inplay live scores from Sportmonks
                var liveScoresData = await _externalApiService.GetInplayLivescoresAsync();
                if (string.IsNullOrEmpty(liveScoresData))
                {
                    return BadRequest("No inplay live scores data received from Sportmonks API");
                }

                // Process and save live scores
                await _dataProcessingService.ProcessAndSaveLiveScoresAsync(liveScoresData);
                
                return Ok(new { 
                    Message = "Inplay live scores synchronized successfully", 
                    Timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing inplay live scores");
                return StatusCode(500, $"Error synchronizing inplay live scores: {ex.Message}");
            }
        }

        [HttpPost("sync/standings/{leagueId}")]
        public async Task<IActionResult> SyncStandings(int leagueId, [FromQuery] int seasonId)
        {
            try
            {
                // Check if league exists
                var league = await _context.Leagues.FindAsync(leagueId);
                if (league == null)
                {
                    return BadRequest($"League with ID {leagueId} not found. Please sync leagues first.");
                }

                // Get standings from Sportmonks
                var standingsData = await _externalApiService.GetStandingsBySeasonIdAsync(seasonId);
                if (string.IsNullOrEmpty(standingsData))
                {
                    return BadRequest("No standings data received from Sportmonks API");
                }

                // Process and save standings
                await _dataProcessingService.ProcessAndSaveStandingsAsync(standingsData, leagueId);
                
                return Ok(new { 
                    Message = "Standings synchronized successfully", 
                    LeagueId = leagueId,
                    SeasonId = seasonId,
                    Timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error synchronizing standings for league {leagueId}");
                return StatusCode(500, $"Error synchronizing standings: {ex.Message}");
            }
        }
    }
}
