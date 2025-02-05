using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TennisClubRanking.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TennisContext>
{
    public TennisContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TennisContext>();
        optionsBuilder.UseInMemoryDatabase("TennisClubMigrations");
        return new TennisContext(optionsBuilder.Options);
    }
}
