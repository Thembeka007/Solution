using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

public class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<SportsBettingDbContext>
{
    public SportsBettingDbContext CreateDbContext(
        string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<
                SportsBettingDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=SportsBettingAI;Username=postgres;Password=Thembeka007");

        return new SportsBettingDbContext(
            optionsBuilder.Options);
    }
}
