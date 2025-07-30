using System.Text.Json;
using System.Text.Json.Serialization;
using Product.Api.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Product.Api.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer redis) : IRedisCacheService
{
    private readonly IDatabase _database = redis.GetDatabase();

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        await _database.StringSetAsync(key, json, expiration);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var data = await _database.StringGetAsync(key);
        if (data.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>(data!);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _database.KeyDeleteAsync(key);
    }
}