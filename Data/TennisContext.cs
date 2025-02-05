using Microsoft.EntityFrameworkCore;
using TennisClubRanking.Models;

namespace TennisClubRanking.Data;

public class TennisContext : DbContext
{
    public TennisContext(DbContextOptions<TennisContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<RankingPoints> RankingPoints { get; set; }
    public DbSet<RankingHistory> RankingHistory { get; set; }
    public DbSet<PromotionRelegation> PromotionRelegations { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        // Configure relationships
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RankingPoints>(entity =>
        {
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(rp => rp.PlayerId);

            entity.HasOne<Match>()
                .WithMany()
                .HasForeignKey(rp => rp.MatchId);
        });

        modelBuilder.Entity<RankingHistory>(entity =>
        {
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(rh => rh.PlayerId);
        });

        modelBuilder.Entity<PromotionRelegation>(entity =>
        {
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(pr => pr.PlayerId);
        });
    }
}
