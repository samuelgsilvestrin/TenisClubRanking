using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TennisClubRanking.Data;
using TennisClubRanking.Models;
using TennisClubRanking.Models.ViewModels;

namespace TennisClubRanking.Services
{
    public class StatisticsService
    {
        private readonly TennisContext _context;
        private const int TOP_PLAYERS_COUNT = 16;

        public StatisticsService(TennisContext context)
        {
            _context = context;
        }

        public async Task<LeaderboardViewModel> GetLeaderboardAsync(string timeFrame = "all", string selectedGender = "all")
        {
            var now = DateTime.UtcNow;
            var startDate = timeFrame.ToLower() switch
            {
                "month" => now.AddMonths(-1),
                "week" => now.AddDays(-7),
                _ => DateTime.MinValue
            };

            var players = await _context.Players
                .Include(p => p.HomeMatches)
                .Include(p => p.AwayMatches)
                .Include(p => p.WonMatches)
                .Include(p => p.PointsHistory)
                .Where(p => p.IsActive)
                .Select(p => new LeaderboardEntry
                {
                    PlayerId = p.Id,
                    PlayerName = $"{p.FirstName} {p.LastName}",
                    RankingPoints = p.RankingPoints,
                    MatchesWon = p.WonMatches.Count(),
                    MatchesLost = p.HomeMatches.Count() + p.AwayMatches.Count() - p.WonMatches.Count(),
                    Gender = p.Gender
                })
                .ToListAsync();

            // Sort by ranking points and assign ranks
            players = players.OrderByDescending(p => p.RankingPoints).ToList();

            var malePlayers = players.Where(p => p.Gender == Gender.Male)
                .OrderByDescending(p => p.RankingPoints)
                .ToList();

            var femalePlayers = players.Where(p => p.Gender == Gender.Female)
                .OrderByDescending(p => p.RankingPoints)
                .ToList();

            return new LeaderboardViewModel
            {
                MaleAdvancedPlayers = malePlayers.Take(TOP_PLAYERS_COUNT).ToList(),
                MaleMediumPlayers = malePlayers.Skip(TOP_PLAYERS_COUNT).ToList(),
                
                FemaleAdvancedPlayers = femalePlayers.Take(TOP_PLAYERS_COUNT).ToList(),
                FemaleMediumPlayers = femalePlayers.Skip(TOP_PLAYERS_COUNT).ToList(),
                
                TimeFrame = timeFrame,
                SelectedGender = selectedGender
            };
        }

        private async Task<List<RankingHistory>> GetPreviousRanks(DateTime date)
        {
            return await _context.RankingHistory
                .Where(rh => rh.RecordedDate.Date == date.Date)
                .ToListAsync();
        }

        public async Task<List<RankingPoints>> GetPlayerPointsHistory(int playerId)
        {
            return await _context.RankingPoints
                .Include(rp => rp.Match)
                .Where(rp => rp.PlayerId == playerId)
                .OrderByDescending(rp => rp.DateEarned)
                .ToListAsync();
        }

        public async Task<List<RankingHistory>> GetPlayerRankingHistory(int playerId)
        {
            return await _context.RankingHistory
                .Where(rh => rh.PlayerId == playerId)
                .OrderByDescending(rh => rh.RecordedDate)
                .ToListAsync();
        }

        public async Task UpdateRankingHistory()
        {
            var players = await _context.Players
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.RankingPoints)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var rankingHistory = players.Select((player, index) => new RankingHistory
            {
                PlayerId = player.Id,
                Rank = index + 1,
                TotalPoints = player.RankingPoints,
                RecordedDate = today
            }).ToList();

            _context.RankingHistory.AddRange(rankingHistory);
            await _context.SaveChangesAsync();
        }
    }
}
