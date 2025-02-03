using System;

namespace TennisClubRanking.Models
{
    public class RankingHistory
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public int Rank { get; set; }
        public int TotalPoints { get; set; }
        public DateTime RecordedDate { get; set; }
    }
}
