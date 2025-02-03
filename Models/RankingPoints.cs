using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TennisClubRanking.Models
{
    public class RankingPoints
    {
        public int Id { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [ForeignKey("PlayerId")]
        public Player Player { get; set; }

        [Required]
        public int MatchId { get; set; }

        [ForeignKey("MatchId")]
        public Match Match { get; set; }

        [Required]
        public int Points { get; set; }

        [Required]
        public DateTime DateEarned { get; set; }

        public bool IsWinnerPoints { get; set; }
    }
}
