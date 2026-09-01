using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(
            this IServiceCollection services,
            string connectionString)
    {
        services.AddDbContext<
            SportsBettingDbContext>(
            options =>
            {
                options.UseNpgsql(
                    connectionString);
            });

        services.AddScoped<
            MatchRepository>();

        return services;
    }
}