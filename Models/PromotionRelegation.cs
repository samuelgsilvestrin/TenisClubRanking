using System;

namespace TennisClubRanking.Models
{
    public class PromotionRelegation
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public DateTime Date { get; set; }
        public bool IsPromotion { get; set; }  // true for promotion, false for relegation
        public string Season { get; set; }  // e.g., "2025-1" for first half of 2025
        public Gender Gender { get; set; }
    }
}
