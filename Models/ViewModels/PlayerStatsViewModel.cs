using System;

namespace TennisClubRanking.Models.ViewModels
{
    public class PlayerStatsViewModel
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int CurrentRank { get; set; }
        public int TotalPoints { get; set; }
        public int MatchesPlayed { get; set; }
        public int MatchesWon { get; set; }
        public double WinRate { get; set; }
        public int PointsThisMonth { get; set; }
        public int RankChange { get; set; }
    }
}
