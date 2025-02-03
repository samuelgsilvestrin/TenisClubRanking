using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TennisClubRanking.Models
{
    public enum MatchStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    }

    public enum MatchType
    {
        Singles,
        Doubles
    }

    public enum Court
    {
        Court1,
        Court2,
        Court3,
        Court4
    }

    public class Match
    {
        public Match()
        {
            CreatedAt = DateTime.UtcNow;
            Status = MatchStatus.Scheduled;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Match Type")]
        public MatchType Type { get; set; }

        [Required(ErrorMessage = "Please select Player 1")]
        [Display(Name = "Player 1")]
        public int HomePlayerId { get; set; }

        [Display(Name = "Player 1")]
        [ForeignKey("HomePlayerId")]
        public virtual Player? HomePlayer { get; set; }

        [Required(ErrorMessage = "Please select Player 2")]
        [Display(Name = "Player 2")]
        public int AwayPlayerId { get; set; }

        [Display(Name = "Player 2")]
        [ForeignKey("AwayPlayerId")]
        public virtual Player? AwayPlayer { get; set; }

        public int? Player3Id { get; set; }
        [ForeignKey("Player3Id")]
        public virtual Player? Player3 { get; set; }

        public int? Player4Id { get; set; }
        [ForeignKey("Player4Id")]
        public virtual Player? Player4 { get; set; }

        public int? TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public virtual Tournament? Tournament { get; set; }

        [Required(ErrorMessage = "Please select a court")]
        [Display(Name = "Court")]
        public Court Court { get; set; }

        [Required(ErrorMessage = "Please select date and time")]
        [Display(Name = "Date and Time")]
        public DateTime ScheduledDateTime { get; set; }

        [Required]
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1); // Default duration is 1 hour

        [Required(ErrorMessage = "Please select if the match is valid for ranking")]
        [Display(Name = "Valid for Ranking")]
        public bool ValidForRanking { get; set; }

        // Match Results
        public string? FirstSetScore { get; set; }
        public string? SecondSetScore { get; set; }
        public string? ThirdSetScore { get; set; }
        public int? WinnerId { get; set; }
        [ForeignKey("WinnerId")]
        public virtual Player? Winner { get; set; }

        [Required]
        [Display(Name = "Status")]
        public MatchStatus Status { get; set; }

        // Score format: "6-4,7-5" (first set 6-4, second set 7-5)
        [StringLength(20)]
        public string? Score { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        // Calculated property for match end time
        [NotMapped]
        public DateTime EndDateTime => ScheduledDateTime.Add(Duration);
    }
}
