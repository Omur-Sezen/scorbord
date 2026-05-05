using Microsoft.AspNetCore.Mvc;
using Skorbord.API.Models;
using Skorbord.API.Services;

namespace Skorbord.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly SportScoreService _sportScoreService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(SportScoreService sportScoreService, ILogger<HomeController> logger)
        {
            _sportScoreService = sportScoreService;
            _logger = logger;
        }

        /// <summary>
        /// Ana sayfa — canlı skor tablosu
        /// </summary>
        public async Task<IActionResult> Index(string? sport = null, string? date = null)
        {
            // Varsayılan: tüm sporlar, bugün
            var selectedSport = sport;
            var selectedDate = date ?? DateTime.Now.ToString("yyyy-MM-dd");

            var data = await _sportScoreService.GetLivescoresAsync(selectedSport, selectedDate);

            ViewBag.Data = data;
            ViewBag.SelectedSport = selectedSport ?? "";
            ViewBag.SelectedDate = selectedDate;
            ViewBag.ApiError = data == null;

            return View();
        }

        /// <summary>
        /// Belirli bir ligin maçları
        /// </summary>
        public async Task<IActionResult> Matches(string leagueName, string? sport = null, string? date = null)
        {
            if (string.IsNullOrWhiteSpace(leagueName))
                return RedirectToAction("Index");

            var selectedDate = date ?? DateTime.Now.ToString("yyyy-MM-dd");
            var result = await _sportScoreService.GetLeagueMatchesAsync(leagueName, sport, selectedDate);

            ViewBag.LeagueName = leagueName;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.Summary = result.Summary;
            ViewBag.ApiError = result.League == null;

            return View(result.League);
        }

        /// <summary>
        /// Maç detayı — temel skor ve bilgiler
        /// </summary>
        public async Task<IActionResult> MatchDetail(string id, string? date = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction("Index");

            var selectedDate = date ?? DateTime.Now.ToString("yyyy-MM-dd");
            var result = await _sportScoreService.FindMatchByIdAsync(id, selectedDate);

            if (result == null || result.Value.Match == null)
            {
                TempData["Error"] = "Maç bulunamadı veya API yanıt vermedi.";
                return RedirectToAction("Index");
            }

            ViewBag.Match = result.Value.Match;
            ViewBag.League = result.Value.League;
            ViewBag.SelectedDate = selectedDate;

            return View(result.Value.Match);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
