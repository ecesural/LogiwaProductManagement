using System.Text.Json;
using Product.Api.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Product.Api.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer redis) : IRedisCacheService
{
    private readonly IDatabase _database = redis.GetDatabase();

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, expiration);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var data = await _database.StringGetAsync(key);
        if (data.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>(data!);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}