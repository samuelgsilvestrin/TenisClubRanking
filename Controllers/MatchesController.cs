using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TennisClubRanking.Data;
using TennisClubRanking.Models;
using TennisClubRanking.Models.ViewModels;
using MatchType = TennisClubRanking.Models.MatchType;
using MatchStatus = TennisClubRanking.Models.MatchStatus;
using Microsoft.AspNetCore.Authorization;

namespace TennisClubRanking.Controllers
{
    [Authorize]
    public class MatchesController : Controller
    {
        private readonly TennisContext _context;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(TennisContext context, ILogger<MatchesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Matches
        public async Task<IActionResult> Index()
        {
            var matches = await _context.Matches
                .Include(m => m.HomePlayer)
                .Include(m => m.AwayPlayer)
                .Include(m => m.Player3)
                .Include(m => m.Player4)
                .Include(m => m.Tournament)
                .Include(m => m.Winner)
                .OrderByDescending(m => m.ScheduledDateTime)
                .ToListAsync();
            return View(matches);
        }

        // GET: Matches/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var players = await _context.Players
                    .OrderBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.FullName
                    })
                    .ToListAsync();

                ViewBag.Players = players;

                _logger.LogInformation($"Found {players.Count} players");
                if (players.Any())
                {
                    _logger.LogInformation($"First player: {players.First().Text}");
                }
                else
                {
                    _logger.LogWarning("No players found in database");
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading players for match creation");
                throw;
            }
        }

        // POST: Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Match match)
        {
            try
            {
                var logMessage = $"Creating match: Home={match.HomePlayerId}, Away={match.AwayPlayerId}, Court={match.Court}, Time={match.ScheduledDateTime}, ValidForRanking={match.ValidForRanking}";
                _logger.LogInformation(logMessage);
                System.IO.File.AppendAllText("match_creation.log", $"{DateTime.Now}: {logMessage}\n");

                // Check for overlapping matches on the same court
                var matchStartTime = match.ScheduledDateTime;
                var matchEndTime = matchStartTime.AddHours(1); // 1-hour duration

                var overlappingMatch = await _context.Matches
                    .Where(m => m.Court == match.Court)
                    .Where(m => (
                        // Match starts during another match
                        (matchStartTime >= m.ScheduledDateTime && matchStartTime < m.ScheduledDateTime.AddHours(1)) ||
                        // Match ends during another match
                        (matchEndTime > m.ScheduledDateTime && matchEndTime <= m.ScheduledDateTime.AddHours(1)) ||
                        // Match completely overlaps another match
                        (matchStartTime <= m.ScheduledDateTime && matchEndTime >= m.ScheduledDateTime.AddHours(1))
                    ))
                    .FirstOrDefaultAsync();

                if (overlappingMatch != null)
                {
                    ModelState.AddModelError("", $"Court {match.Court} is already booked from {overlappingMatch.ScheduledDateTime:HH:mm} to {overlappingMatch.ScheduledDateTime.AddHours(1):HH:mm}");
                    var players = await _context.Players
                        .OrderBy(p => p.FirstName)
                        .ThenBy(p => p.LastName)
                        .Select(p => new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = p.FullName
                        })
                        .ToListAsync();

                    ViewBag.Players = players;
                    return View(match);
                }

                // Check if players are already in a match at this time
                var playerOverlappingMatch = await _context.Matches
                    .Where(m => (m.HomePlayerId == match.HomePlayerId || 
                               m.AwayPlayerId == match.HomePlayerId || 
                               m.HomePlayerId == match.AwayPlayerId || 
                               m.AwayPlayerId == match.AwayPlayerId))
                    .Where(m => (
                        // Match starts during another match
                        (matchStartTime >= m.ScheduledDateTime && matchStartTime < m.ScheduledDateTime.AddHours(1)) ||
                        // Match ends during another match
                        (matchEndTime > m.ScheduledDateTime && matchEndTime <= m.ScheduledDateTime.AddHours(1)) ||
                        // Match completely overlaps another match
                        (matchStartTime <= m.ScheduledDateTime && matchEndTime >= m.ScheduledDateTime.AddHours(1))
                    ))
                    .Include(m => m.HomePlayer)
                    .Include(m => m.AwayPlayer)
                    .FirstOrDefaultAsync();

                if (playerOverlappingMatch != null)
                {
                    var busyPlayer = playerOverlappingMatch.HomePlayerId == match.HomePlayerId || 
                                   playerOverlappingMatch.HomePlayerId == match.AwayPlayerId 
                                   ? playerOverlappingMatch.HomePlayer.FullName 
                                   : playerOverlappingMatch.AwayPlayer.FullName;

                    ModelState.AddModelError("", $"Player {busyPlayer} already has a match from {playerOverlappingMatch.ScheduledDateTime:HH:mm} to {playerOverlappingMatch.ScheduledDateTime.AddHours(1):HH:mm}");
                    var players = await _context.Players
                        .OrderBy(p => p.FirstName)
                        .ThenBy(p => p.LastName)
                        .Select(p => new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = p.FullName
                        })
                        .ToListAsync();

                    ViewBag.Players = players;
                    return View(match);
                }

                // Set default values
                match.Type = MatchType.Singles;
                match.Status = MatchStatus.Scheduled;
                match.Duration = TimeSpan.FromHours(1);

                // Log the match object state
                var matchState = $"Match state before save: Type={match.Type}, Status={match.Status}, Duration={match.Duration}";
                _logger.LogInformation(matchState);
                System.IO.File.AppendAllText("match_creation.log", $"{DateTime.Now}: {matchState}\n");

                try
                {
                    _logger.LogInformation("Adding match to context");
                    _context.Add(match);

                    _logger.LogInformation("Saving changes to database");
                    await _context.SaveChangesAsync();
                    
                    var successMessage = $"Match created successfully with ID: {match.Id}";
                    _logger.LogInformation(successMessage);
                    System.IO.File.AppendAllText("match_creation.log", $"{DateTime.Now}: {successMessage}\n");
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception dbEx)
                {
                    var dbError = $"Database error: {dbEx.Message}\nStack trace: {dbEx.StackTrace}";
                    _logger.LogError(dbEx, dbError);
                    System.IO.File.AppendAllText("match_creation.log", $"{DateTime.Now}: {dbError}\n");
                    
                    TempData["Error"] = "Failed to save match to database. Please try again.";
                    ModelState.AddModelError("", dbEx.Message);
                    
                    var players = await _context.Players
                        .OrderBy(p => p.FirstName)
                        .ThenBy(p => p.LastName)
                        .Select(p => new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = p.FullName
                        })
                        .ToListAsync();

                    ViewBag.Players = players;
                    return View(match);
                }
            }
            catch (Exception ex)
            {
                var error = $"Error creating match: {ex.Message}\nStack trace: {ex.StackTrace}";
                _logger.LogError(ex, error);
                System.IO.File.AppendAllText("match_creation.log", $"{DateTime.Now}: {error}\n");
                
                TempData["Error"] = "An unexpected error occurred. Please try again.";
                ModelState.AddModelError("", ex.Message);
                
                var players = await _context.Players
                    .OrderBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.FullName
                    })
                    .ToListAsync();

                ViewBag.Players = players;
                return View(match);
            }
        }

        // GET: Matches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.HomePlayer)
                .Include(m => m.AwayPlayer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            await PrepareCreateView();
            return View(match);
        }

        // POST: Matches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HomePlayerId,AwayPlayerId,Court,ScheduledDateTime,ValidForRanking")] Match match)
        {
            if (id != match.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMatch = await _context.Matches
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (existingMatch == null)
                    {
                        return NotFound();
                    }

                    // Preserve values that shouldn't be modified through edit
                    match.Status = existingMatch.Status;
                    match.FirstSetScore = existingMatch.FirstSetScore;
                    match.SecondSetScore = existingMatch.SecondSetScore;
                    match.ThirdSetScore = existingMatch.ThirdSetScore;
                    match.WinnerId = existingMatch.WinnerId;
                    match.Duration = existingMatch.Duration;

                    _context.Update(match);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(match.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PrepareCreateView();
            return View(match);
        }

        // GET: Matches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.HomePlayer)
                .Include(m => m.AwayPlayer)
                .Include(m => m.Player3)
                .Include(m => m.Player4)
                .Include(m => m.Winner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // GET: Matches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.HomePlayer)
                .Include(m => m.AwayPlayer)
                .Include(m => m.Winner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        // POST: Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Matches/Result/5
        public async Task<IActionResult> Result(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.HomePlayer)
                .Include(m => m.AwayPlayer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            var viewModel = new MatchResultViewModel
            {
                MatchId = match.Id,
                HomePlayerName = match.HomePlayer?.FullName ?? "Unknown",
                AwayPlayerName = match.AwayPlayer?.FullName ?? "Unknown",
                ScheduledDateTime = match.ScheduledDateTime
            };

            return View(viewModel);
        }

        // POST: Matches/Result/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Result(int id, MatchResultViewModel viewModel)
        {
            if (id != viewModel.MatchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var match = await _context.Matches
                        .Include(m => m.HomePlayer)
                        .Include(m => m.AwayPlayer)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (match == null)
                    {
                        return NotFound();
                    }

                    match.FirstSetScore = viewModel.FirstSetScore;
                    match.SecondSetScore = viewModel.SecondSetScore;
                    match.ThirdSetScore = viewModel.ThirdSetScore;
                    match.Status = MatchStatus.Completed;

                    // Determine winner based on set scores
                    var homePlayerSets = 0;
                    var awayPlayerSets = 0;

                    var firstSetScoreParts = viewModel.FirstSetScore.Split('-');
                    if (int.Parse(firstSetScoreParts[0]) > int.Parse(firstSetScoreParts[1]))
                        homePlayerSets++;
                    else
                        awayPlayerSets++;

                    var secondSetScoreParts = viewModel.SecondSetScore.Split('-');
                    if (int.Parse(secondSetScoreParts[0]) > int.Parse(secondSetScoreParts[1]))
                        homePlayerSets++;
                    else
                        awayPlayerSets++;

                    if (!string.IsNullOrEmpty(viewModel.ThirdSetScore))
                    {
                        var thirdSetScoreParts = viewModel.ThirdSetScore.Split('-');
                        if (int.Parse(thirdSetScoreParts[0]) > int.Parse(thirdSetScoreParts[1]))
                            homePlayerSets++;
                        else
                            awayPlayerSets++;
                    }

                    match.WinnerId = homePlayerSets > awayPlayerSets ? match.HomePlayerId : match.AwayPlayerId;

                    // Update ranking points if match is valid for ranking
                    if (match.ValidForRanking)
                    {
                        var winner = await _context.Players.FindAsync(match.WinnerId);
                        if (winner != null)
                        {
                            winner.RankingPoints += 10; // Add 10 points for a win
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating match result");
                    ModelState.AddModelError("", "Error updating match result. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            var match2 = await _context.Matches
                .Include(m => m.HomePlayer)
                .Include(m => m.AwayPlayer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match2 != null)
            {
                viewModel.HomePlayerName = match2.HomePlayer?.FullName ?? "Unknown";
                viewModel.AwayPlayerName = match2.AwayPlayer?.FullName ?? "Unknown";
            }

            return View(viewModel);
        }

        // GET: Matches/SaveResult
        public async Task<IActionResult> SaveResult(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var match = await _context.Matches
                .Include(m => m.HomePlayer)
                .Include(m => m.AwayPlayer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            // Check if result is already entered
            if (match.FirstSetScore != null)
            {
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new MatchResultViewModel
            {
                MatchId = match.Id,
                HomePlayerName = match.HomePlayer?.FullName ?? "Unknown",
                AwayPlayerName = match.AwayPlayer?.FullName ?? "Unknown",
                ScheduledDateTime = match.ScheduledDateTime
            };

            return View(viewModel);
        }

        // POST: Matches/SaveResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(int id, [Bind("MatchId,FirstSetScore,SecondSetScore,ThirdSetScore")] MatchResultViewModel viewModel)
        {
            if (id != viewModel.MatchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var match = await _context.Matches
                        .Include(m => m.HomePlayer)
                        .Include(m => m.AwayPlayer)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (match == null)
                    {
                        return NotFound();
                    }

                    match.FirstSetScore = viewModel.FirstSetScore;
                    match.SecondSetScore = viewModel.SecondSetScore;
                    match.ThirdSetScore = viewModel.ThirdSetScore;
                    match.Status = MatchStatus.Completed;

                    // Calculate winner based on sets won
                    var homePlayerSets = 0;
                    var awayPlayerSets = 0;

                    var firstSetScoreParts = viewModel.FirstSetScore?.Split('-');
                    if (firstSetScoreParts?.Length == 2 && int.TryParse(firstSetScoreParts[0], out int homeScore1) && int.TryParse(firstSetScoreParts[1], out int awayScore1))
                    {
                        if (homeScore1 > awayScore1)
                            homePlayerSets++;
                        else
                            awayPlayerSets++;
                    }

                    var secondSetScoreParts = viewModel.SecondSetScore?.Split('-');
                    if (secondSetScoreParts?.Length == 2 && int.TryParse(secondSetScoreParts[0], out int homeScore2) && int.TryParse(secondSetScoreParts[1], out int awayScore2))
                    {
                        if (homeScore2 > awayScore2)
                            homePlayerSets++;
                        else
                            awayPlayerSets++;
                    }

                    if (!string.IsNullOrEmpty(viewModel.ThirdSetScore))
                    {
                        var thirdSetScoreParts = viewModel.ThirdSetScore.Split('-');
                        if (thirdSetScoreParts.Length == 2 && int.TryParse(thirdSetScoreParts[0], out int homeScore3) && int.TryParse(thirdSetScoreParts[1], out int awayScore3))
                        {
                            if (homeScore3 > awayScore3)
                                homePlayerSets++;
                            else
                                awayPlayerSets++;
                        }
                    }

                    match.WinnerId = homePlayerSets > awayPlayerSets ? match.HomePlayerId : match.AwayPlayerId;

                    // Update ranking points if match is valid for ranking
                    if (match.ValidForRanking)
                    {
                        var winner = await _context.Players.FindAsync(match.WinnerId);
                        if (winner != null)
                        {
                            winner.RankingPoints += 10;
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving match result");
                    ModelState.AddModelError("", "Error saving match result. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            var match2 = await _context.Matches
                .Include(m => m.HomePlayer)
                .Include(m => m.AwayPlayer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match2 != null)
            {
                viewModel.HomePlayerName = match2.HomePlayer?.FullName ?? "Unknown";
                viewModel.AwayPlayerName = match2.AwayPlayer?.FullName ?? "Unknown";
            }

            return View(viewModel);
        }

        private bool MatchExists(int id)
        {
            return _context.Matches.Any(e => e.Id == id);
        }

        private async Task PrepareCreateView()
        {
            var players = await _context.Players
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.FullName
                })
                .ToListAsync();

            ViewBag.Players = players;
        }
    }
}
