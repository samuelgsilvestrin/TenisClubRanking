using Microsoft.AspNetCore.Mvc;
using TennisClubRanking.Models.ViewModels;
using TennisClubRanking.Services;
using Microsoft.Extensions.Logging;
using TennisClubRanking.Data;
using Microsoft.EntityFrameworkCore;

namespace TennisClubRanking.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly TennisContext _context;

        public AuthController(AuthService authService, ILogger<AuthController> logger, TennisContext context)
        {
            _authService = authService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login()
        {
            Console.WriteLine("\n=== CHECKING DATABASE USERS ===");
            // Debug: Check all users in database
            var users = await _context.Users.ToListAsync();
            Console.WriteLine($"Total users in database: {users.Count}");
            foreach (var user in users)
            {
                Console.WriteLine($"User found - Email: {user.Email}, Username: {user.Username}, IsActive: {user.IsActive}");
            }
            Console.WriteLine("============================\n");

            // Check if user is already logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");
            Console.WriteLine($"Current session - UserId: {userId}, Username: {username}");

            if (userId.HasValue)
            {
                Console.WriteLine($"User already logged in, redirecting to home");
                return RedirectToAction("Index", "Home");
            }
            
            Console.WriteLine("Displaying login page");
            return View();
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            Console.WriteLine($"\n=== LOGIN ATTEMPT ===");
            Console.WriteLine($"Email: {model.Email}");

            // Debug: Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                Console.WriteLine($"No user found with email: {model.Email}");
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View(model);
            }

            Console.WriteLine($"Found user - Email: {user.Email}, Username: {user.Username}, IsActive: {user.IsActive}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Invalid model state during login");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model error: {error.ErrorMessage}");
                }
                return View(model);
            }

            var (success, message) = await _authService.Login(model.Email, model.Password, model.RememberMe);
            
            if (!success)
            {
                Console.WriteLine($"Login failed: {message}");
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            Console.WriteLine($"Login successful for {model.Email}, redirecting to home");

            // Manually set session values here as well
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            // Check if session was set correctly
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");
            Console.WriteLine($"Session values after login - UserId: {userId}, Username: {username}");
            Console.WriteLine("===================\n");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            Console.WriteLine($"\n=== REGISTRATION ATTEMPT ===");
            Console.WriteLine($"Email: {model.Email}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Invalid model state during registration");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model error: {error.ErrorMessage}");
                }
                return View(model);
            }

            var (success, message) = await _authService.Register(
                model.Email,
                model.Username,
                model.Password,
                model.FirstName,
                model.LastName
            );

            if (!success)
            {
                Console.WriteLine($"Registration failed: {message}");
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            Console.WriteLine($"Registration successful for {model.Email}, redirecting to login");
            Console.WriteLine("=========================\n");
            return RedirectToAction(nameof(Login));
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("Username");
            Console.WriteLine($"User {username} logging out");
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
