using Microsoft.AspNetCore.SignalR;

namespace Skorbord.API.Hubs
{
    public class MatchHub : Hub
    {
        // Senior .NET Dev: Named according to requested standards
        public async Task SendScoreUpdate(int matchId, string homeTeam, string awayTeam, int homeScore, int awayScore, string status, string time)
        {
            await Clients.All.SendAsync("ReceiveScoreUpdate", matchId, homeTeam, awayTeam, homeScore, awayScore, status, time);
        }

        public async Task SendGoalAlert(string message, string teamName, int newScore)
        {
            await Clients.All.SendAsync("ReceiveGoalAlert", message, teamName, newScore);
        }
    }
}
