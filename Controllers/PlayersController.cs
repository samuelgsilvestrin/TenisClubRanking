using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TennisClubRanking.Data;
using TennisClubRanking.Models;
using Microsoft.AspNetCore.Authorization;

namespace TennisClubRanking.Controllers
{
    [Authorize]
    public class PlayersController : Controller
    {
        private readonly TennisContext _context;

        public PlayersController(TennisContext context)
        {
            _context = context;
        }

        // GET: Players
        public async Task<IActionResult> Index()
        {
            var players = await _context.Players
                .Include(p => p.HomeMatches)
                .Include(p => p.AwayMatches)
                .Include(p => p.WonMatches)
                .OrderByDescending(p => p.RankingPoints)
                .ThenBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
            return View(players);
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.HomeMatches)
                    .ThenInclude(m => m.AwayPlayer)
                .Include(p => p.HomeMatches)
                    .ThenInclude(m => m.Tournament)
                .Include(p => p.AwayMatches)
                    .ThenInclude(m => m.HomePlayer)
                .Include(p => p.AwayMatches)
                    .ThenInclude(m => m.Tournament)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // GET: Players/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Players/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,DateOfBirth,Gender,PhoneNumber,Email")] Player player)
        {
            if (ModelState.IsValid)
            {
                player.RegistrationDate = DateTime.Now;
                player.RankingPoints = 0; 
                player.MatchesWon = 0;
                player.MatchesLost = 0;
                player.Ranking = await _context.Players.CountAsync() + 1; // Set initial ranking
                
                _context.Add(player);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(player);
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            return View(player);
        }

        // POST: Players/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,DateOfBirth,Gender,PhoneNumber,Email,RankingPoints")] Player player)
        {
            if (id != player.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPlayer = await _context.Players.FindAsync(id);
                    if (existingPlayer == null)
                    {
                        return NotFound();
                    }

                    existingPlayer.FirstName = player.FirstName;
                    existingPlayer.LastName = player.LastName;
                    existingPlayer.DateOfBirth = player.DateOfBirth;
                    existingPlayer.Gender = player.Gender;
                    existingPlayer.PhoneNumber = player.PhoneNumber;
                    existingPlayer.Email = player.Email;
                    existingPlayer.RankingPoints = player.RankingPoints;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(player);
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.HomeMatches)
                .Include(p => p.AwayMatches)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (player == null)
            {
                return NotFound();
            }

            // Check if player has any matches
            if (player.HomeMatches.Any() || player.AwayMatches.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete player with existing matches. Please remove the matches first.";
                return RedirectToAction(nameof(Index));
            }

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players
                .Include(p => p.HomeMatches)
                .Include(p => p.AwayMatches)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (player == null)
            {
                return NotFound();
            }

            // Double check for matches
            if (player.HomeMatches.Any() || player.AwayMatches.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete player with existing matches. Please remove the matches first.";
                return RedirectToAction(nameof(Index));
            }

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.Id == id);
        }
    }
}
