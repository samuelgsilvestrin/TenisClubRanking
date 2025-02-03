using System.ComponentModel.DataAnnotations;

namespace TennisClubRanking.Models
{
    public class TournamentPlayer
    {
        public int TournamentId { get; set; }
        public virtual Tournament Tournament { get; set; } = null!;

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; } = null!;

        public DateTime RegistrationDate { get; set; }

        public int? Seed { get; set; }

        public TournamentPlayerStatus Status { get; set; }
    }

    public enum TournamentPlayerStatus
    {
        Registered,
        Active,
        Eliminated,
        Winner
    }
}
