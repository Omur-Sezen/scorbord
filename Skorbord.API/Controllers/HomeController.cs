using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skorbord.API.Data;
using Skorbord.API.DTOs;
using Skorbord.API.Services;

namespace Skorbord.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly SkorbordDbContext _context;
        private readonly StandingsService _standingsService;

        public HomeController(SkorbordDbContext context, StandingsService standingsService)
        {
            _context = context;
            _standingsService = standingsService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var leagues = await _context.Leagues
                    .Select(l => new { l.Id, l.Name, l.Country, l.Season })
                    .ToListAsync();

                ViewBag.Leagues = leagues;
            }
            catch (Exception ex)
            {
                // Handle database connection issues gracefully
                ViewBag.Leagues = new List<object>();
                ViewBag.DatabaseError = "Veritabanı bağlantısı şu anda mevcut değil. Lütfen daha sonra tekrar deneyin.";
                Console.WriteLine($"Database error in Index: {ex.Message}");
            }
            
            return View();
        }

        public async Task<IActionResult> Standings(int leagueId)
        {
            try
            {
                var standings = await _standingsService.GetLeagueStandingsAsync(leagueId);
                return View(standings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error in Standings: {ex.Message}");
                TempData["Error"] = "Veritabanı bağlantısı şu anda mevcut değil. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Matches(int leagueId)
        {
            try
            {
                var matches = await _context.Matches
                    .Include(m => m.HomeTeam)
                    .Include(m => m.AwayTeam)
                    .Include(m => m.League)
                    .Where(m => m.LeagueId == leagueId)
                    .OrderByDescending(m => m.MatchDate)
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

                ViewBag.League = await _context.Leagues.FindAsync(leagueId);
                return View(matches);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error in Matches: {ex.Message}");
                TempData["Error"] = "Veritabanı bağlantısı şu anda mevcut değil. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
