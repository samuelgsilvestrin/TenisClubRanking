using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TennisClubRanking.Models
{
    public class Player
    {
        public Player()
        {
            HomeMatches = new List<Match>();
            AwayMatches = new List<Match>();
            WonMatches = new List<Match>();
            PointsHistory = new List<RankingPoints>();
            RankingHistory = new List<RankingHistory>();
            TournamentPlayers = new List<TournamentPlayer>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Photo URL")]
        public string? PhotoUrl { get; set; }

        [Required]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Required]
        [Display(Name = "Ranking Points")]
        public int RankingPoints { get; set; }

        public int Ranking { get; set; }

        public int MatchesWon { get; set; }

        public int MatchesLost { get; set; }

        // Navigation properties
        public virtual ICollection<Match> HomeMatches { get; set; }
        public virtual ICollection<Match> AwayMatches { get; set; }
        public virtual ICollection<Match> WonMatches { get; set; }
        public virtual ICollection<RankingPoints> PointsHistory { get; set; }
        public virtual ICollection<RankingHistory> RankingHistory { get; set; }
        public virtual ICollection<TournamentPlayer> TournamentPlayers { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }
}
