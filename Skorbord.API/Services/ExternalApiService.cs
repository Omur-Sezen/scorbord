using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Skorbord.API.Services
{
    public class ExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalApiService> _logger;
        private readonly string _apiToken;
        private readonly string _baseUrl;

        public ExternalApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ExternalApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _apiToken = _configuration["ApiSports:ApiKey"] ?? throw new InvalidOperationException("API key not configured");
            _baseUrl = _configuration["ApiSports:BaseUrl"] ?? "https://api.sportmonks.com/v3/football/";

            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        private string BuildUrl(string endpoint, Dictionary<string, string>? parameters = null)
        {
            var url = $"{endpoint}?api_token={_apiToken}";
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    url += $"&{param.Key}={Uri.EscapeDataString(param.Value)}";
                }
            }
            return url;
        }

        public async Task<string> GetLeaguesAsync()
        {
            try
            {
                var url = BuildUrl("leagues", new Dictionary<string, string> { ["include"] = "country" });
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully retrieved leagues from Sportmonks");
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leagues from Sportmonks");
                throw;
            }
        }

        public async Task<string> GetTeamsBySeasonIdAsync(int seasonId)
        {
            try
            {
                var url = BuildUrl($"teams/seasons/{seasonId}", new Dictionary<string, string> { ["include"] = "country;venue" });
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Successfully retrieved teams for season {seasonId}");
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving teams for season {seasonId}");
                throw;
            }
        }

        public async Task<string> GetFixturesBySeasonIdAsync(int seasonId)
        {
            try
            {
                // Sportmonks uses filters=seasons:seasonID format
                var url = $"fixtures?api_token={_apiToken}&include=participants;scores;state&per_page=50&filters=seasons:{seasonId}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Successfully retrieved fixtures for season {seasonId}");
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving fixtures for season {seasonId}");
                throw;
            }
        }

        public async Task<string> GetLiveScoresAsync()
        {
            try
            {
                var url = BuildUrl("livescores", new Dictionary<string, string> { ["include"] = "participants;scores;state" });
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully retrieved live scores");
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving live scores");
                throw;
            }
        }

        public async Task<string> GetInplayLivescoresAsync()
        {
            try
            {
                // Rich includes for inplay data: participants, scores, state, events, statistics
                var url = BuildUrl("livescores/inplay", new Dictionary<string, string> 
                { 
                    ["include"] = "participants;scores;state;events;statistics;venue;league;season;round"
                });
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully retrieved inplay live scores with rich includes");
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inplay live scores");
                throw;
            }
        }

        public async Task<string> GetStandingsBySeasonIdAsync(int seasonId)
        {
            try
            {
                // Sportmonks standings endpoint with season filter
                var url = $"standings?api_token={_apiToken}&filters=standingSeasons:{seasonId}&include=participant;details";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Successfully retrieved standings for season {seasonId}");
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving standings for season {seasonId}");
                throw;
            }
        }

        // LEGACY METHODS for backward compatibility
        public async Task<string> GetCompetitionsAsync() => await GetLeaguesAsync();
        
        public async Task<string> GetFixturesByLeagueIdAsync(int leagueId, int season)
        {
            var seasonId = GetSeasonIdFromLeagueId(leagueId);
            return seasonId > 0 ? await GetFixturesBySeasonIdAsync(seasonId) : string.Empty;
        }

        public async Task<string> GetTeamsByLeagueIdAsync(int leagueId, int season)
        {
            var seasonId = GetSeasonIdFromLeagueId(leagueId);
            return seasonId > 0 ? await GetTeamsBySeasonIdAsync(seasonId) : string.Empty;
        }

        private int GetSeasonIdFromLeagueId(int leagueId)
        {
            var mapping = new Dictionary<int, int>
            {
                { 39, 21646 },   // Premier League 2023/24
                { 135, 22814 },  // Serie A 2023/24
                { 78, 22919 },   // Bundesliga 2023/24
                { 140, 22538 },  // La Liga 2023/24
                { 61, 22805 },   // Ligue 1 2023/24
            };
            return mapping.TryGetValue(leagueId, out var seasonId) ? seasonId : 0;
        }
    }
}
