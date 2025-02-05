using Microsoft.EntityFrameworkCore;
using TennisClubRanking.Models;

namespace TennisClubRanking.Data
{
    public class TennisContext : DbContext
    {
        public TennisContext(DbContextOptions<TennisContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentPlayer> TournamentPlayers { get; set; }
        public DbSet<RankingPoints> RankingPoints { get; set; }
        public DbSet<RankingHistory> RankingHistory { get; set; }
        public DbSet<PromotionRelegation> PromotionRelegations { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TournamentPlayer>()
                .HasKey(tp => new { tp.TournamentId, tp.PlayerId });

            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomePlayer)
                .WithMany(p => p.HomeMatches)
                .HasForeignKey(m => m.HomePlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.AwayPlayer)
                .WithMany(p => p.AwayMatches)
                .HasForeignKey(m => m.AwayPlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Player3)
                .WithMany()
                .HasForeignKey(m => m.Player3Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Player4)
                .WithMany()
                .HasForeignKey(m => m.Player4Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Winner)
                .WithMany(p => p.WonMatches)
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RankingPoints>()
                .HasOne(rp => rp.Player)
                .WithMany(p => p.PointsHistory)
                .HasForeignKey(rp => rp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RankingPoints>()
                .HasOne(rp => rp.Match)
                .WithMany()
                .HasForeignKey(rp => rp.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PromotionRelegation>()
                .HasOne(pr => pr.Player)
                .WithMany()
                .HasForeignKey(pr => pr.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure auto-increment columns for MySQL
            if (Database.ProviderName == "Pomelo.EntityFrameworkCore.MySql")
            {
                foreach (var entity in modelBuilder.Model.GetEntityTypes())
                {
                    // Configure auto-increment for Id properties
                    var properties = entity.GetProperties()
                        .Where(p => p.Name == "Id" && p.ClrType == typeof(int));
                    
                    foreach (var property in properties)
                    {
                        property.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
                    }
                }
            }

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Player1)
                .WithMany(p => p.MatchesAsPlayer1)
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Player2)
                .WithMany(p => p.MatchesAsPlayer2)
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RankingPoints>()
                .HasOne(rp => rp.Player)
                .WithMany(p => p.RankingPoints)
                .HasForeignKey(rp => rp.PlayerId);

            modelBuilder.Entity<RankingPoints>()
                .HasOne(rp => rp.Match)
                .WithMany(m => m.RankingPoints)
                .HasForeignKey(rp => rp.MatchId);

            modelBuilder.Entity<RankingHistory>()
                .HasOne(rh => rh.Player)
                .WithMany(p => p.RankingHistory)
                .HasForeignKey(rh => rh.PlayerId);

            modelBuilder.Entity<PromotionRelegation>()
                .HasOne(pr => pr.Player)
                .WithMany(p => p.PromotionRelegations)
                .HasForeignKey(pr => pr.PlayerId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
