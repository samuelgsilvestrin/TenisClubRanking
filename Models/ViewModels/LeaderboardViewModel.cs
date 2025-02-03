using TennisClubRanking.Models;

namespace TennisClubRanking.Models.ViewModels
{
    public class LeaderboardViewModel
    {
        public IEnumerable<LeaderboardEntry> MaleAdvancedPlayers { get; set; } = new List<LeaderboardEntry>();
        public IEnumerable<LeaderboardEntry> MaleMediumPlayers { get; set; } = new List<LeaderboardEntry>();
        public IEnumerable<LeaderboardEntry> FemaleAdvancedPlayers { get; set; } = new List<LeaderboardEntry>();
        public IEnumerable<LeaderboardEntry> FemaleMediumPlayers { get; set; } = new List<LeaderboardEntry>();
        
        public string TimeFrame { get; set; } = string.Empty;
        public string SelectedGender { get; set; } = string.Empty;
    }
}
