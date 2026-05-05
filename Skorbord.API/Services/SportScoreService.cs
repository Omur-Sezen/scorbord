using Skorbord.API.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Skorbord.API.Services
{
    public class SportScoreService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SportScoreService> _logger;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _apiHost;

        public SportScoreService(HttpClient httpClient, IConfiguration configuration, ILogger<SportScoreService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["SportScoreApi:BaseUrl"] ?? "https://sportscore1.p.rapidapi.com";
            _apiKey = configuration["SportScoreApi:Key"] ?? "aed0babbaemsh5afd1bd466a0a95p127c4ejsnde2d22a19537";
            _apiHost = configuration["SportScoreApi:Host"] ?? "sportscore1.p.rapidapi.com";
            
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", _apiHost);
        }

        public async Task<LivescoreViewModel?> GetLivescoresAsync(string? sport = null, string? date = null)
        {
            try
            {
                var sportId = GetSportId(sport);
                var url = "";
                bool isToday = string.IsNullOrEmpty(date) || date == DateTime.Now.ToString("yyyy-MM-dd");

                // If no specific date or it's today, we could use events/live or events/date/{date}
                // The documentation says GET /sports/{id}/events/date/{date}?page=1
                
                string dateStr = date ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                
                // For simplicity, we fetch all sports if sport is not specified
                // But the API requires a sport ID for the date endpoint: /sports/{id}/events/date/{date}
                // Alternatively: GET /events/date/{date}?page=1 or POST /events/search
                // Let's use GET /events/date/{date}?page=1 as per common usage
                
                url = $"{_baseUrl}/events/date/{dateStr}?page=1";
                
                _logger.LogInformation("SportScore API isteği gönderiliyor: {Url}", url);

                var response = await _httpClient.GetFromJsonAsync<SportScoreResponse<List<SportScoreEvent>>>(url);

                if (response?.Data == null)
                    return null;

                var events = response.Data;
                
                if (sportId > 0)
                {
                    events = events.Where(e => e.SportId == sportId).ToList();
                }

                return MapToViewModel(events, dateStr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SportScore verisi çekilirken hata: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<(LeagueViewModel? League, LivescoresSummary? Summary)> GetLeagueMatchesAsync(
            string leagueName, string? sport = null, string? date = null)
        {
            var data = await GetLivescoresAsync(sport, date);
            if (data == null) return (null, null);

            var league = data.Leagues.FirstOrDefault(l =>
                l.League.Equals(leagueName, StringComparison.OrdinalIgnoreCase));

            return (league, data.Summary);
        }

        public async Task<(MatchViewModel? Match, LeagueViewModel? League)?> FindMatchByIdAsync(
            string matchId, string? date = null)
        {
            try
            {
                // We can query the specific event endpoint GET /events/{id}
                var url = $"{_baseUrl}/events/{matchId}";
                var response = await _httpClient.GetFromJsonAsync<SportScoreResponse<SportScoreEvent>>(url);
                
                if (response?.Data == null)
                    return null;
                    
                var evt = response.Data;
                var viewMatch = MapEventToMatchViewModel(evt);
                
                var viewLeague = new LeagueViewModel
                {
                    League = evt.League?.Name ?? "Bilinmiyor",
                    Country = "",
                    Sport = GetSportName(evt.SportId),
                    Matches = new List<MatchViewModel> { viewMatch }
                };

                return (viewMatch, viewLeague);
            }
            catch (Exception)
            {
                // Fallback to searching in the date
                var data = await GetLivescoresAsync(date: date);
                if (data == null) return null;

                foreach (var league in data.Leagues)
                {
                    var match = league.Matches.FirstOrDefault(m => m.Id == matchId);
                    if (match != null)
                        return (match, league);
                }

                return null;
            }
        }
        
        private LivescoreViewModel MapToViewModel(List<SportScoreEvent> events, string date)
        {
            var vm = new LivescoreViewModel
            {
                Date = date,
                FetchedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var leaguesDict = new Dictionary<int, LeagueViewModel>();

            int liveCount = 0;
            int finishedCount = 0;
            int scheduledCount = 0;

            foreach (var evt in events)
            {
                var matchVm = MapEventToMatchViewModel(evt);
                
                if (matchVm.IsLive) liveCount++;
                else if (matchVm.State == "post") finishedCount++;
                else scheduledCount++;

                if (evt.League != null)
                {
                    if (!leaguesDict.TryGetValue(evt.League.Id, out var leagueVm))
                    {
                        leagueVm = new LeagueViewModel
                        {
                            League = evt.League.Name,
                            Country = "", // SportScore API might have country in section, but we don't have it directly in the event model above.
                            Sport = GetSportName(evt.SportId)
                        };
                        leaguesDict[evt.League.Id] = leagueVm;
                    }
                    leagueVm.Matches.Add(matchVm);
                }
            }

            vm.Leagues = leaguesDict.Values.OrderBy(l => l.League).ToList();
            vm.Summary = new LivescoresSummary
            {
                TotalMatches = events.Count,
                LiveMatches = liveCount,
                FinishedMatches = finishedCount,
                ScheduledMatches = scheduledCount,
                TotalLeagues = leaguesDict.Count
            };

            return vm;
        }

        private MatchViewModel MapEventToMatchViewModel(SportScoreEvent evt)
        {
            bool isLive = evt.Status == "inprogress" || evt.StatusMore == "HT" || evt.Status == "interrupted";
            bool isFinished = evt.Status == "finished" || evt.Status == "ended";
            string state = isFinished ? "post" : (isLive ? "in_progress" : "pre");
            
            // Format time from start_at (e.g. "2024-01-15 14:30:00")
            string timeStr = "";
            if (!string.IsNullOrEmpty(evt.StartAt) && evt.StartAt.Length >= 16)
            {
                // Simple parsing assumption "YYYY-MM-DD HH:mm:ss"
                var parts = evt.StartAt.Split(' ');
                if (parts.Length == 2)
                {
                    var timeParts = parts[1].Split(':');
                    if (timeParts.Length >= 2)
                    {
                        timeStr = $"{timeParts[0]}:{timeParts[1]}";
                    }
                }
            }

            return new MatchViewModel
            {
                Id = evt.Id.ToString(),
                HomeTeam = evt.HomeTeam?.Name ?? "Bilinmiyor",
                AwayTeam = evt.AwayTeam?.Name ?? "Bilinmiyor",
                HomeScore = evt.HomeScore?.Current ?? evt.HomeScore?.Display,
                AwayScore = evt.AwayScore?.Current ?? evt.AwayScore?.Display,
                HalfTimeScore = new HalfTimeScore 
                { 
                    Home = evt.HomeScore?.Period1 ?? 0, 
                    Away = evt.AwayScore?.Period1 ?? 0 
                },
                Status = string.IsNullOrEmpty(evt.StatusMore) ? evt.Status : evt.StatusMore,
                State = state,
                StatusCode = evt.Status,
                StartTime = string.IsNullOrEmpty(timeStr) ? "00:00" : timeStr,
                IsLive = isLive,
                MatchUrl = ""
            };
        }

        private int GetSportId(string? sport)
        {
            if (string.IsNullOrEmpty(sport)) return 0;
            return sport.ToLower() switch
            {
                "soccer" or "football" => 1,
                "tennis" => 2,
                "basketball" => 3,
                "ice hockey" => 4,
                "volleyball" => 5,
                "handball" => 6,
                _ => 0
            };
        }

        private string GetSportName(int sportId)
        {
            return sportId switch
            {
                1 => "Futbol",
                2 => "Tenis",
                3 => "Basketbol",
                4 => "Buz Hokeyi",
                5 => "Voleybol",
                6 => "Hentbol",
                _ => "Diğer"
            };
        }
    }
}
