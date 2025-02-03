namespace TennisClubRanking.Models.ViewModels
{
    public class LeaderboardEntry
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int RankingPoints { get; set; }
        public int MatchesWon { get; set; }
        public int MatchesLost { get; set; }
        public Gender Gender { get; set; }
        public int Ranking { get; set; }
    }
}
