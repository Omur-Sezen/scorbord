using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Skorbord.API.Services
{
    public class ScoreUpdateBackgroundService : BackgroundService
    {
        private readonly ILogger<ScoreUpdateBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly int _updateIntervalMinutes;

        public ScoreUpdateBackgroundService(
            ILogger<ScoreUpdateBackgroundService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _updateIntervalMinutes = int.Parse(_configuration["BackgroundService:UpdateIntervalMinutes"] ?? "5");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Score Update Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Running score update task at: {DateTime.UtcNow}");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var externalApiService = scope.ServiceProvider.GetRequiredService<ExternalApiService>();
                        var dataProcessingService = scope.ServiceProvider.GetRequiredService<DataProcessingService>();

                        // Get live scores
                        var liveScoresData = await externalApiService.GetLiveScoresAsync();
                        if (!string.IsNullOrEmpty(liveScoresData))
                        {
                            await ProcessLiveScoresAsync(liveScoresData, dataProcessingService);
                        }

                        // Update fixtures for major leagues
                        await UpdateMajorLeaguesAsync(externalApiService, dataProcessingService);
                    }

                    _logger.LogInformation($"Score update task completed at: {DateTime.UtcNow}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during score update task.");
                }

                // Wait for the specified interval
                await Task.Delay(TimeSpan.FromMinutes(_updateIntervalMinutes), stoppingToken);
            }
        }

        private async Task ProcessLiveScoresAsync(string liveScoresData, DataProcessingService dataProcessingService)
        {
            try
            {
                // Process live scores using Sportmonks data
                await dataProcessingService.ProcessAndSaveLiveScoresAsync(liveScoresData);
                _logger.LogInformation("Live scores data received and processed from Sportmonks");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing live scores from Sportmonks");
            }
        }

        private async Task UpdateMajorLeaguesAsync(ExternalApiService externalApiService, DataProcessingService dataProcessingService)
        {
            try
            {
                // Free plan only includes Denmark Superliga and Scotland Premiership
                var majorLeagues = new[]
                {
                    new { LeagueId = 271, Name = "Superliga (Danimarka)", SeasonId = 25536 },  // Current season 2025/2026
                    new { LeagueId = 501, Name = "Premiership (İskoçya)", SeasonId = 12963 }   // Scotland - need to verify
                };

                foreach (var league in majorLeagues)
                {
                    try
                    {
                        _logger.LogInformation($"Updating {league.Name} (ID: {league.LeagueId}, Season: {league.SeasonId})");

                        using var serviceScope = _serviceProvider.CreateScope();
                        var apiService = serviceScope.ServiceProvider.GetRequiredService<ExternalApiService>();
                        var processingService = serviceScope.ServiceProvider.GetRequiredService<DataProcessingService>();
                        
                        // Get teams using Sportmonks season ID
                        var teamsData = await apiService.GetTeamsBySeasonIdAsync(league.SeasonId);
                        if (!string.IsNullOrEmpty(teamsData))
                        {
                            await processingService.ProcessAndSaveTeamsAsync(teamsData, league.LeagueId);
                            _logger.LogInformation($"Successfully updated teams for {league.Name}");
                        }

                        // Get fixtures using Sportmonks season ID
                        var fixturesData = await apiService.GetFixturesBySeasonIdAsync(league.SeasonId);
                        if (!string.IsNullOrEmpty(fixturesData))
                        {
                            await processingService.ProcessAndSaveMatchesAsync(fixturesData, league.LeagueId);
                            _logger.LogInformation($"Successfully updated {league.Name}");
                        }
                        else
                        {
                            _logger.LogWarning($"No data received for {league.Name}");
                        }

                        // Get standings using Sportmonks season ID
                        var standingsData = await apiService.GetStandingsBySeasonIdAsync(league.SeasonId);
                        if (!string.IsNullOrEmpty(standingsData))
                        {
                            await processingService.ProcessAndSaveStandingsAsync(standingsData, league.LeagueId);
                            _logger.LogInformation($"Successfully updated standings for {league.Name}");
                        }
                        else
                        {
                            _logger.LogWarning($"No standings data received for {league.Name}");
                        }

                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating {league.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating major leagues");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Score Update Background Service is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}
