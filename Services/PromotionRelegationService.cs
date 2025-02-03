using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TennisClubRanking.Data;
using TennisClubRanking.Models;

namespace TennisClubRanking.Services
{
    public class PromotionRelegationService
    {
        private readonly TennisContext _context;
        private const int PROMOTION_RELEGATION_COUNT = 5;
        private const int TOP_PLAYERS_COUNT = 16;

        public PromotionRelegationService(TennisContext context)
        {
            _context = context;
        }

        public string GetCurrentSeason()
        {
            var now = DateTime.UtcNow;
            var halfYear = now.Month <= 6 ? 1 : 2;
            return $"{now.Year}-{halfYear}";
        }

        public async Task<bool> IsPromotionRelegationDue()
        {
            var now = DateTime.UtcNow;
            var currentSeason = GetCurrentSeason();

            // Check if we already did promotion/relegation for this season
            var lastPromotion = await _context.PromotionRelegations
                .OrderByDescending(pr => pr.Date)
                .FirstOrDefaultAsync();

            if (lastPromotion == null)
                return true;

            return lastPromotion.Season != currentSeason;
        }

        public async Task ProcessPromotionsAndRelegations()
        {
            if (!await IsPromotionRelegationDue())
                return;

            var currentSeason = GetCurrentSeason();
            var now = DateTime.UtcNow;

            // Process male players
            await ProcessGenderPromotionsAndRelegations(Gender.Male, currentSeason, now);

            // Process female players
            await ProcessGenderPromotionsAndRelegations(Gender.Female, currentSeason, now);

            await _context.SaveChangesAsync();
        }

        private async Task ProcessGenderPromotionsAndRelegations(Gender gender, string season, DateTime date)
        {
            var players = await _context.Players
                .Where(p => p.IsActive && p.Gender == gender)
                .OrderByDescending(p => p.RankingPoints)
                .ToListAsync();

            var advancedPlayers = players.Take(TOP_PLAYERS_COUNT).ToList();
            var intermediatePlayers = players.Skip(TOP_PLAYERS_COUNT).ToList();

            // Get bottom 5 from advanced
            var playersToRelegate = advancedPlayers
                .Skip(TOP_PLAYERS_COUNT - PROMOTION_RELEGATION_COUNT)
                .Take(PROMOTION_RELEGATION_COUNT);

            // Get top 5 from intermediate
            var playersToPromote = intermediatePlayers
                .Take(PROMOTION_RELEGATION_COUNT);

            // Record relegations
            foreach (var player in playersToRelegate)
            {
                _context.PromotionRelegations.Add(new PromotionRelegation
                {
                    PlayerId = player.Id,
                    Date = date,
                    IsPromotion = false,
                    Season = season,
                    Gender = gender
                });
            }

            // Record promotions
            foreach (var player in playersToPromote)
            {
                _context.PromotionRelegations.Add(new PromotionRelegation
                {
                    PlayerId = player.Id,
                    Date = date,
                    IsPromotion = true,
                    Season = season,
                    Gender = gender
                });
            }
        }
    }
}
