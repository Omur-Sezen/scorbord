using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skorbord.API.Data;
using Skorbord.API.Models;

namespace Skorbord.API.Services
{
    public class DataProcessingService
    {
        private readonly SkorbordDbContext _context;
        private readonly ILogger<DataProcessingService> _logger;

        public DataProcessingService(SkorbordDbContext context, ILogger<DataProcessingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessAndSaveLeaguesAsync(string jsonContent)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;
                
                // Sportmonks format: { "data": [...] }
                if (root.TryGetProperty("data", out var dataElement))
                {
                    foreach (var league in dataElement.EnumerateArray())
                    {
                        try
                        {
                            var leagueId = league.GetProperty("id").GetInt32();
                            var leagueName = league.GetProperty("name").GetString() ?? "";
                            
                            // Get country from nested property
                            var countryName = "Unknown";
                            if (league.TryGetProperty("country", out var countryElement))
                            {
                                countryName = countryElement.GetProperty("name").GetString() ?? "Unknown";
                            }
                            
                            // Get current season
                            var season = "2023-2024"; // Default
                            if (league.TryGetProperty("current_season_id", out var currentSeasonElement))
                            {
                                var currentSeasonId = currentSeasonElement.GetInt32();
                                // Could fetch season details here if needed
                            }

                            if (string.IsNullOrEmpty(leagueName))
                            {
                                _logger.LogWarning("Skipping league due to missing name");
                                continue;
                            }

                            var leagueEntity = new League
                            {
                                Id = leagueId,
                                Name = leagueName,
                                Country = countryName,
                                Season = season,
                                CreatedAt = DateTime.UtcNow
                            };

                            var existingLeague = await _context.Leagues
                                .FirstOrDefaultAsync(l => l.Id == leagueEntity.Id);

                            if (existingLeague == null)
                            {
                                _context.Leagues.Add(leagueEntity);
                                _logger.LogInformation($"Added new league: {leagueEntity.Name} (ID: {leagueId})");
                            }
                            else
                            {
                                // Update existing league
                                existingLeague.Name = leagueEntity.Name;
                                existingLeague.Country = leagueEntity.Country;
                                existingLeague.UpdatedAt = DateTime.UtcNow;
                                _logger.LogInformation($"Updated league: {leagueEntity.Name} (ID: {leagueId})");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing individual league element");
                            continue;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Leagues processed and saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing leagues data");
                throw;
            }
        }

        public async Task ProcessAndSaveTeamsAsync(string jsonContent, int leagueId)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;
                
                // Sportmonks format: { "data": [...] }
                if (root.TryGetProperty("data", out var dataElement))
                {
                    foreach (var teamElement in dataElement.EnumerateArray())
                    {
                        try
                        {
                            var teamId = teamElement.GetProperty("id").GetInt32();
                            var teamName = teamElement.GetProperty("name").GetString() ?? "";
                            
                            // Get short name/code
                            var shortName = teamName.Substring(0, Math.Min(3, teamName.Length)).ToUpper();
                            if (teamElement.TryGetProperty("short_code", out var shortCodeElement))
                            {
                                shortName = shortCodeElement.GetString() ?? shortName;
                            }
                            
                            // Get logo URL
                            var logoUrl = "";
                            if (teamElement.TryGetProperty("image_path", out var imagePathElement))
                            {
                                logoUrl = imagePathElement.GetString() ?? "";
                            }
                            
                            // Get founded year
                            var foundedYear = 0;
                            if (teamElement.TryGetProperty("founded", out var foundedElement))
                            {
                                foundedYear = foundedElement.GetInt32();
                            }
                            
                            // Get stadium from venue
                            var stadium = "";
                            if (teamElement.TryGetProperty("venue", out var venueElement))
                            {
                                stadium = venueElement.GetProperty("name").GetString() ?? "";
                            }
                            
                            var team = new Team
                            {
                                // Don't set Id - let SQL Server auto-generate it
                                Name = teamName,
                                ShortName = shortName,
                                LogoUrl = logoUrl,
                                Stadium = stadium,
                                FoundedYear = foundedYear,
                                LeagueId = leagueId,
                                CreatedAt = DateTime.UtcNow
                            };

                            var existingTeam = await _context.Teams
                                .FirstOrDefaultAsync(t => t.Id == team.Id);

                            if (existingTeam == null)
                            {
                                _context.Teams.Add(team);
                                _logger.LogInformation($"Added new team: {team.Name} (ID: {teamId})");
                            }
                            else
                            {
                                // Update existing team
                                existingTeam.Name = team.Name;
                                existingTeam.ShortName = team.ShortName;
                                existingTeam.LogoUrl = team.LogoUrl;
                                existingTeam.Stadium = team.Stadium;
                                existingTeam.UpdatedAt = DateTime.UtcNow;
                                _logger.LogInformation($"Updated team: {team.Name} (ID: {teamId})");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing individual team element");
                            continue;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Teams processed and saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing teams data");
                throw;
            }
        }

        public async Task ProcessAndSaveMatchesAsync(string jsonContent, int leagueId)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;
                
                // Sportmonks format: { "data": [...] }
                if (root.TryGetProperty("data", out var dataElement))
                {
                    foreach (var fixture in dataElement.EnumerateArray())
                    {
                        try
                        {
                            var fixtureId = fixture.GetProperty("id").GetInt32();
                            
                            // Get participants (teams)
                            if (!fixture.TryGetProperty("participants", out var participantsElement))
                                continue;
                            
                            var participants = participantsElement.EnumerateArray().ToList();
                            if (participants.Count < 2) continue;
                            
                            var homeTeam = participants[0];
                            var awayTeam = participants[1];
                            
                            var homeTeamName = homeTeam.GetProperty("name").GetString() ?? "";
                            var awayTeamName = awayTeam.GetProperty("name").GetString() ?? "";
                            
                            // Lookup teams by name in database (Sportmonks IDs don't match our DB IDs)
                            var homeTeamId = await GetTeamIdByName(homeTeamName, leagueId);
                            var awayTeamId = await GetTeamIdByName(awayTeamName, leagueId);
                            
                            // Skip if teams not found in database
                            if (homeTeamId == 0 || awayTeamId == 0)
                            {
                                _logger.LogWarning($"Teams not found for match: {homeTeamName} vs {awayTeamName}. Skipping.");
                                continue;
                            }
                            
                            // Parse match date
                            var matchDate = DateTime.UtcNow;
                            if (fixture.TryGetProperty("starting_at", out var startingAtElement))
                            {
                                DateTime.TryParse(startingAtElement.GetString(), out matchDate);
                            }
                            
                            // Get scores from scores array
                            var homeScore = 0;
                            var awayScore = 0;
                            if (fixture.TryGetProperty("scores", out var scoresElement))
                            {
                                foreach (var score in scoresElement.EnumerateArray())
                                {
                                    if (score.TryGetProperty("participant_id", out var participantIdElement))
                                    {
                                        var participantId = participantIdElement.GetInt32();
                                        if (score.TryGetProperty("goals", out var goalsElement))
                                        {
                                            var goals = goalsElement.GetInt32();
                                            if (participantId == homeTeamId)
                                                homeScore = goals;
                                            else if (participantId == awayTeamId)
                                                awayScore = goals;
                                        }
                                    }
                                }
                            }
                            
                            // Get match status from state
                            var status = "Scheduled";
                            if (fixture.TryGetProperty("state", out var stateElement))
                            {
                                var stateId = stateElement.GetProperty("id").GetInt32();
                                status = stateId switch
                                {
                                    1 => "Scheduled",
                                    2 => "Live",
                                    3 => "Live",
                                    5 => "Finished",
                                    12 => "Postponed",
                                    13 => "Cancelled",
                                    _ => "Scheduled"
                                };
                            }
                            
                            // Get venue
                            var venue = "";
                            if (fixture.TryGetProperty("venue", out var venueElement))
                            {
                                venue = venueElement.GetProperty("name").GetString() ?? "";
                            }
                            
                            // Get round
                            var round = 0;
                            if (fixture.TryGetProperty("round", out var roundElement))
                            {
                                round = roundElement.GetProperty("id").GetInt32();
                            }

                            var match = new Match
                            {
                                // Don't set Id - let SQL Server auto-generate it
                                HomeTeamId = homeTeamId,
                                AwayTeamId = awayTeamId,
                                LeagueId = leagueId,
                                MatchDate = matchDate,
                                Status = status,
                                HomeScore = homeScore,
                                AwayScore = awayScore,
                                Venue = venue,
                                Round = round,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            // Find existing match by team IDs and date
                            var existingMatch = await _context.Matches
                                .FirstOrDefaultAsync(m => 
                                    m.HomeTeamId == homeTeamId && 
                                    m.AwayTeamId == awayTeamId && 
                                    m.MatchDate.Date == matchDate.Date);

                            if (existingMatch == null)
                            {
                                _context.Matches.Add(match);
                                _logger.LogInformation($"Added new match: {homeTeamName} vs {awayTeamName} (ID: {fixtureId})");
                            }
                            else
                            {
                                // Update existing match
                                existingMatch.Status = match.Status;
                                existingMatch.HomeScore = match.HomeScore;
                                existingMatch.AwayScore = match.AwayScore;
                                existingMatch.UpdatedAt = DateTime.UtcNow;
                                _logger.LogInformation($"Updated match: {homeTeamName} vs {awayTeamName} (ID: {fixtureId})");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing individual match element");
                            continue;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Matches processed and saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing matches data");
                throw;
            }
        }

        private async Task<int> GetTeamIdByName(string teamName, int leagueId)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Name.Contains(teamName) && t.LeagueId == leagueId);
            
            return team?.Id ?? 0;
        }

        private string GetMatchStatus(string apiStatus)
        {
            return apiStatus.ToUpper() switch
            {
                "SCHEDULED" => "Scheduled",
                "LIVE" => "Live",
                "IN_PLAY" => "Live",
                "FINISHED" => "Finished",
                "POSTPONED" => "Postponed",
                "CANCELLED" => "Cancelled",
                _ => "Scheduled"
            };
        }

        public async Task ProcessAndSaveLiveScoresAsync(string jsonContent)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;
                
                // Sportmonks format: { "data": [...] }
                if (root.TryGetProperty("data", out var dataElement))
                {
                    foreach (var fixture in dataElement.EnumerateArray())
                    {
                        try
                        {
                            // Get fixture ID
                            var fixtureId = fixture.GetProperty("id").GetInt32();
                            
                            // Get participants (teams)
                            if (!fixture.TryGetProperty("participants", out var participantsElement))
                                continue;
                            
                            var participants = participantsElement.EnumerateArray().ToList();
                            if (participants.Count < 2) continue;
                            
                            var homeTeam = participants[0];
                            var awayTeam = participants[1];
                            
                            var homeTeamName = homeTeam.GetProperty("name").GetString() ?? "";
                            var awayTeamName = awayTeam.GetProperty("name").GetString() ?? "";
                            var homeTeamId = homeTeam.GetProperty("id").GetInt32();
                            var awayTeamId = awayTeam.GetProperty("id").GetInt32();
                            
                            // Get scores
                            var homeScore = 0;
                            var awayScore = 0;
                            if (fixture.TryGetProperty("scores", out var scoresElement))
                            {
                                foreach (var score in scoresElement.EnumerateArray())
                                {
                                    if (score.TryGetProperty("participant_id", out var participantIdElement))
                                    {
                                        var scoreParticipantId = participantIdElement.GetInt32();
                                        var scoreValue = score.GetProperty("score").GetProperty("goals").GetInt32();
                                        
                                        if (scoreParticipantId == homeTeamId)
                                            homeScore = scoreValue;
                                        else if (scoreParticipantId == awayTeamId)
                                            awayScore = scoreValue;
                                    }
                                }
                            }
                            
                            // Get match status
                            var status = "Scheduled";
                            if (fixture.TryGetProperty("state", out var stateElement))
                            {
                                var stateId = stateElement.GetProperty("id").GetInt32();
                                status = stateId switch
                                {
                                    1 => "Scheduled",
                                    2 => "Live",
                                    3 => "Live",
                                    5 => "Finished",
                                    _ => "Scheduled"
                                };
                            }
                            
                            // Find existing match by team names and date
                            var matchDate = fixture.TryGetProperty("starting_at", out var startingAtElement) 
                                ? DateTime.Parse(startingAtElement.GetString() ?? DateTime.UtcNow.ToString()) 
                                : DateTime.UtcNow;
                            
                            var existingMatch = await _context.Matches
                                .Include(m => m.HomeTeam)
                                .Include(m => m.AwayTeam)
                                .FirstOrDefaultAsync(m => 
                                    (m.HomeTeam.Name == homeTeamName || m.AwayTeam.Name == awayTeamName) &&
                                    m.MatchDate.Date == matchDate.Date);
                            
                            if (existingMatch != null)
                            {
                                // Update existing match
                                existingMatch.HomeScore = homeScore;
                                existingMatch.AwayScore = awayScore;
                                existingMatch.Status = status;
                                existingMatch.UpdatedAt = DateTime.UtcNow;
                                _logger.LogInformation($"Updated live score: {homeTeamName} {homeScore} - {awayScore} {awayTeamName} ({status})");
                            }
                            else
                            {
                                _logger.LogWarning($"Match not found in database: {homeTeamName} vs {awayTeamName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing individual live score element");
                            continue;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Live scores processed and saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing live scores data");
                throw;
            }
        }

        public async Task ProcessAndSaveStandingsAsync(string jsonContent, int leagueId)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;
                
                // Sportmonks format: { "data": [...] }
                if (root.TryGetProperty("data", out var dataElement))
                {
                    foreach (var standing in dataElement.EnumerateArray())
                    {
                        try
                        {
                            // Get participant (team) info
                            if (!standing.TryGetProperty("participant", out var participantElement))
                                continue;
                            
                            var teamName = participantElement.GetProperty("name").GetString() ?? "";
                            
                            // Find team in database
                            var team = await _context.Teams
                                .FirstOrDefaultAsync(t => t.Name == teamName && t.LeagueId == leagueId);
                            
                            if (team == null)
                            {
                                _logger.LogWarning($"Team not found for standings: {teamName}");
                                continue;
                            }
                            
                            // Parse standings data
                            var position = standing.GetProperty("position").GetInt32();
                            var points = standing.GetProperty("points").GetInt32();
                            var played = standing.GetProperty("details")[0].GetProperty("value").GetInt32(); // Played games
                            var won = standing.GetProperty("details")[1].GetProperty("value").GetInt32();
                            var drawn = standing.GetProperty("details")[2].GetProperty("value").GetInt32();
                            var lost = standing.GetProperty("details")[3].GetProperty("value").GetInt32();
                            var goalsFor = standing.GetProperty("details")[4].GetProperty("value").GetInt32();
                            var goalsAgainst = standing.GetProperty("details")[5].GetProperty("value").GetInt32();
                            
                            // Update team with standings info
                            team.Points = points;
                            team.Position = position;
                            team.Played = played;
                            team.Won = won;
                            team.Drawn = drawn;
                            team.Lost = lost;
                            team.GoalsFor = goalsFor;
                            team.GoalsAgainst = goalsAgainst;
                            team.GoalDifference = goalsFor - goalsAgainst;
                            team.UpdatedAt = DateTime.UtcNow;
                            
                            _logger.LogInformation($"Updated standings for {teamName}: Position {position}, Points {points}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing individual standing element");
                            continue;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Standings processed and saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing standings data");
                throw;
            }
        }
    }
}
