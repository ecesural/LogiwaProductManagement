namespace Product.Api.Application.Common;

public static class CacheKeys
{
    public const string ProductAll = "product:all";
    public const string ProductFilter = "product:filter:";
    public static string ProductById(Guid id) => $"product:{id}";
}