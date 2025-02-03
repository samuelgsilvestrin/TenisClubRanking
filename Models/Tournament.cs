using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TennisClubRanking.Models
{
    public class Tournament
    {
        public Tournament()
        {
            Matches = new List<Match>();
            TournamentPlayers = new List<TournamentPlayer>();
            CreatedAt = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public TournamentStatus Status { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        public int? WinnerId { get; set; }
        public virtual Player? Winner { get; set; }

        // Navigation properties
        public virtual ICollection<TournamentPlayer> TournamentPlayers { get; set; }
        public virtual ICollection<Match> Matches { get; set; }
    }

    public enum TournamentStatus
    {
        [Display(Name = "Registration Open")]
        RegistrationOpen,

        [Display(Name = "Registration Closed")]
        RegistrationClosed,

        [Display(Name = "In Progress")]
        InProgress,

        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "Cancelled")]
        Cancelled
    }
}
