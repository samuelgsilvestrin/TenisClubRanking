using TennisClubRanking.Data;
using TennisClubRanking.Models;
using Microsoft.EntityFrameworkCore;

namespace TennisClubRanking.Services
{
    public class RankingService
    {
        private readonly TennisContext _context;

        public RankingService(TennisContext context)
        {
            _context = context;
        }

        public async Task AwardMatchPoints(Match match)
        {
            if (match.Status != MatchStatus.Completed || !match.ValidForRanking || match.WinnerId == null)
                return;

            // Points for winner
            var winnerPoints = new RankingPoints
            {
                PlayerId = match.WinnerId.Value,
                MatchId = match.Id,
                Points = 10, // Winner gets 10 points
                DateEarned = DateTime.UtcNow,
                IsWinnerPoints = true
            };

            // Points for loser (participation points)
            var loserId = match.WinnerId == match.HomePlayerId ? match.AwayPlayerId : match.HomePlayerId;
            var loserPoints = new RankingPoints
            {
                PlayerId = loserId,
                MatchId = match.Id,
                Points = 2, // Loser gets 2 points for participating
                DateEarned = DateTime.UtcNow,
                IsWinnerPoints = false
            };

            // Add points to database
            await _context.RankingPoints.AddRangeAsync(winnerPoints, loserPoints);

            // Update player total points
            var winner = await _context.Players.FindAsync(match.WinnerId);
            var loser = await _context.Players.FindAsync(loserId);

            if (winner != null)
                winner.RankingPoints += winnerPoints.Points;
            if (loser != null)
                loser.RankingPoints += loserPoints.Points;

            // Update rankings
            await UpdateRankings();

            // Save changes
            await _context.SaveChangesAsync();
        }

        private async Task UpdateRankings()
        {
            var players = await _context.Players
                .OrderByDescending(p => p.RankingPoints)
                .ToListAsync();

            for (int i = 0; i < players.Count; i++)
            {
                players[i].Ranking = i + 1;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Player>> GetRankings()
        {
            return await _context.Players
                .OrderByDescending(p => p.RankingPoints)
                .ThenBy(p => p.FullName)
                .ToListAsync();
        }
    }
}
