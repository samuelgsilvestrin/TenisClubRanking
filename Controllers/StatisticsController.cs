using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TennisClubRanking.Services;

namespace TennisClubRanking.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly StatisticsService _statisticsService;

        public StatisticsController(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        public async Task<IActionResult> Leaderboard(string timeFrame = "all")
        {
            var viewModel = await _statisticsService.GetLeaderboardAsync(timeFrame);
            return View(viewModel);
        }

        public async Task<IActionResult> PlayerPointsHistory(int id)
        {
            var history = await _statisticsService.GetPlayerPointsHistory(id);
            return View(history);
        }

        public async Task<IActionResult> PlayerRankingHistory(int id)
        {
            var history = await _statisticsService.GetPlayerRankingHistory(id);
            return View(history);
        }
    }
}
