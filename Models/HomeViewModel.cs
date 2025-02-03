namespace TennisClubRanking.Models
{
    public class HomeViewModel
    {
        public List<Player> TopPlayers { get; set; } = new();
        public List<Tournament> UpcomingTournaments { get; set; } = new();
        public List<Match> RecentMatches { get; set; } = new();
    }
}
