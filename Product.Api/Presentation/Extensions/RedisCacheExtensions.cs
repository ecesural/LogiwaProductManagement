using System.Collections;
using Product.Api.Application.Common;
using Product.Api.Application.Common.Interfaces;

namespace Product.Api.Presentation.Extensions;

public static class RedisCacheExtensions
{
    public static async Task<(T Result, bool FromCache)> TryGetOrSetAsync<T>(
        this IRedisCacheService cacheService,
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan duration,
        CancellationToken cancellationToken = default,
        bool cacheIfEmpty = true)
    {
        var cached = await cacheService.GetAsync<T>(key, cancellationToken);
        if (cached is not null)
            return (cached, true);

        var result = await factory(cancellationToken);

        if (result is null) return (result, false);
        if (cacheIfEmpty || !IsEmptyCollection(result))
        {
            await cacheService.SetAsync(key, result, duration, cancellationToken);
        }

        return (result, false);
    }

    private static bool IsEmptyCollection<T>(T value)
    {
        if (value is IEnumerable enumerable)
            return !enumerable.Cast<object>().Any();
        return false;
    }
    
    public static async Task RemoveProductCacheAsync<T>(
        this IRedisCacheService redisCacheService,
        Guid productId,
        ILoggerService<T> logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.WhenAll(
                redisCacheService.RemoveAsync(CacheKeys.ProductById(productId), cancellationToken),
                redisCacheService.RemoveAsync(CacheKeys.ProductAll, cancellationToken)
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Redis cache silinirken hata oluştu: {ex.Message}");
        }
    }
}