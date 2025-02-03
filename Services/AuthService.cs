using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TennisClubRanking.Data;
using TennisClubRanking.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TennisClubRanking.Services
{
    public class AuthService
    {
        private readonly TennisContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(TennisContext context, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<User?> GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return null;

            return await _context.Users.FindAsync(userId.Value);
        }

        public async Task<(bool success, string message)> Register(string email, string username, string password, string? firstName, string? lastName)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return (false, "Email already registered");

            if (await _context.Users.AnyAsync(u => u.Username == username))
                return (false, "Username already taken");

            var user = new User
            {
                Email = email,
                Username = username,
                PasswordHash = HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Registration successful");
        }

        public async Task<(bool success, string message)> Login(string email, string password, bool rememberMe)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning($"Login attempt failed: User not found for email {email}");
                    return (false, "Invalid email or password");
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login attempt failed: Invalid password for email {email}");
                    return (false, "Invalid email or password");
                }

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogError("HttpContext is null during login attempt");
                    return (false, "An error occurred during login");
                }

                // Set session with user information
                httpContext.Session.Clear(); // Clear any existing session
                httpContext.Session.SetInt32("UserId", user.Id);
                httpContext.Session.SetString("Username", user.Username);
                _logger.LogInformation($"Session set - UserId: {user.Id}, Username: {user.Username}");

                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(2)
                };

                // Sign in the user
                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (rememberMe)
                {
                    // Set persistent cookie for remember me functionality
                    var options = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(30),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    };
                    httpContext.Response.Cookies.Append("RememberMe", user.Id.ToString(), options);
                }

                _logger.LogInformation($"User {user.Username} (ID: {user.Id}) logged in successfully");
                return (true, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process");
                return (false, "An error occurred during login");
            }
        }

        public async Task Logout()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                httpContext.Session.Clear();
                httpContext.Response.Cookies.Delete("RememberMe");
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == storedHash;
        }
    }
}
