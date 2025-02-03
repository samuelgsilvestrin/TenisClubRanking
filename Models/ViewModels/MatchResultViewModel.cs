using System.ComponentModel.DataAnnotations;

namespace TennisClubRanking.Models.ViewModels
{
    public class MatchResultViewModel
    {
        public int MatchId { get; set; }
        public string HomePlayerName { get; set; } = string.Empty;
        public string AwayPlayerName { get; set; } = string.Empty;
        public DateTime ScheduledDateTime { get; set; }

        [Required(ErrorMessage = "Please enter the first set score")]
        [RegularExpression(@"^[0-7]-[0-7]$", ErrorMessage = "Score must be in format '6-4', '7-5', '7-6', etc.")]
        [Display(Name = "First Set")]
        public string FirstSetScore { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the second set score")]
        [RegularExpression(@"^[0-7]-[0-7]$", ErrorMessage = "Score must be in format '6-4', '7-5', '7-6', etc.")]
        [Display(Name = "Second Set")]
        public string SecondSetScore { get; set; } = string.Empty;

        [RegularExpression(@"^[0-7]-[0-7]$", ErrorMessage = "Score must be in format '6-4', '7-5', '7-6', etc.")]
        [Display(Name = "Third Set (if played)")]
        public string? ThirdSetScore { get; set; }

        public int? WinnerId { get; set; }
    }
}
