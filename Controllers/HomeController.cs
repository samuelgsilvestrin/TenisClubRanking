using Microsoft.AspNetCore.Mvc;
using TennisClubRanking.Models;
using System.Diagnostics;
using TennisClubRanking.Services;

namespace TennisClubRanking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthService _authService;

        public HomeController(ILogger<HomeController> logger, AuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public IActionResult Index()
        {
            try
            {
                _logger.LogInformation("Accessing Index page");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing Index page");
                return Error();
            }
        }

        public IActionResult Privacy()
        {
            try
            {
                _logger.LogInformation("Accessing Privacy page");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing Privacy page");
                return Error();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error page accessed. RequestId: {RequestId}", requestId);
            return View(new ErrorViewModel { RequestId = requestId });
        }

        [Route("/health")]
        public IActionResult Health()
        {
            try
            {
                return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { status = "unhealthy", error = ex.Message });
            }
        }
    }
}
