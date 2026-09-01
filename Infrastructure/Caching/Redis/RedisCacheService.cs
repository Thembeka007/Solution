using StackExchange.Redis;

namespace Infrastructure.Caching.Redis;

public class RedisCacheService
{
    private readonly IDatabase _database;

    public RedisCacheService(
        string connectionString)
    {
        var redis =
            ConnectionMultiplexer
                .Connect(connectionString);

        _database = redis.GetDatabase();
    }

    public async Task SetAsync(
        string key,
        string value,
        TimeSpan expiry)
    {
        await _database.StringSetAsync(
            key,
            value,
            expiry);
    }

    public async Task<string?>
        GetAsync(string key)
    {
        return await _database
            .StringGetAsync(key);
    }
}