using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using PlatformApp.Application.Catalog;
using PlatformApp.Domain.Catalog;

namespace PlatformApp.Infrastructure.Caching;

/// <summary>
/// Caching decorator over <see cref="ICatalogRepository"/>. Catalog reads (brands,
/// types) are cached in <see cref="IDistributedCache"/> — backed by Redis in
/// production, or an in-memory distributed cache for local/dev. Writes invalidate
/// the affected cache keys. KG target stack: "Redis (StackExchange.Redis)".
/// </summary>
public sealed class CachedCatalogRepository : ICatalogRepository
{
    private const string BrandsKey = "catalog:brands";
    private const string TypesKey  = "catalog:types";
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    private readonly ICatalogRepository _inner;
    private readonly IDistributedCache _cache;

    public CachedCatalogRepository(ICatalogRepository inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<CatalogBrand>> ListBrandsAsync(CancellationToken cancellationToken)
    {
        var cached = await _cache.GetStringAsync(BrandsKey, cancellationToken);
        if (cached is not null)
        {
            var dtos = JsonSerializer.Deserialize<List<BrandRecord>>(cached) ?? [];
            return dtos.Select(d => CatalogBrand.Restore(d.Id, d.Name)).ToArray();
        }

        var brands = await _inner.ListBrandsAsync(cancellationToken);
        var payload = JsonSerializer.Serialize(brands.Select(b => new BrandRecord(b.Id, b.Name)));
        await _cache.SetStringAsync(BrandsKey, payload, CacheOptions, cancellationToken);
        return brands;
    }

    public async Task<IReadOnlyCollection<CatalogType>> ListTypesAsync(CancellationToken cancellationToken)
    {
        var cached = await _cache.GetStringAsync(TypesKey, cancellationToken);
        if (cached is not null)
        {
            var dtos = JsonSerializer.Deserialize<List<TypeRecord>>(cached) ?? [];
            return dtos.Select(d => CatalogType.Restore(d.Id, d.Name)).ToArray();
        }

        var types = await _inner.ListTypesAsync(cancellationToken);
        var payload = JsonSerializer.Serialize(types.Select(t => new TypeRecord(t.Id, t.Name)));
        await _cache.SetStringAsync(TypesKey, payload, CacheOptions, cancellationToken);
        return types;
    }

    // Paged item queries are not cached (high cardinality); delegate straight through.
    public Task<(IReadOnlyCollection<CatalogItem> Items, int TotalCount)> ListItemsAsync(CatalogItemsQuery query, CancellationToken cancellationToken)
        => _inner.ListItemsAsync(query, cancellationToken);

    public Task<CatalogItem?> GetItemByIdAsync(Guid catalogItemId, CancellationToken cancellationToken)
        => _inner.GetItemByIdAsync(catalogItemId, cancellationToken);

    public async Task<CatalogItem> AddItemAsync(CatalogItem item, CancellationToken cancellationToken)
    {
        var result = await _inner.AddItemAsync(item, cancellationToken);
        await InvalidateAsync(cancellationToken);
        return result;
    }

    public async Task<CatalogItem> UpdateItemAsync(CatalogItem item, CancellationToken cancellationToken)
    {
        var result = await _inner.UpdateItemAsync(item, cancellationToken);
        await InvalidateAsync(cancellationToken);
        return result;
    }

    public async Task<bool> DeleteItemAsync(Guid catalogItemId, CancellationToken cancellationToken)
    {
        var result = await _inner.DeleteItemAsync(catalogItemId, cancellationToken);
        await InvalidateAsync(cancellationToken);
        return result;
    }

    private async Task InvalidateAsync(CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(BrandsKey, cancellationToken);
        await _cache.RemoveAsync(TypesKey, cancellationToken);
    }

    private sealed record BrandRecord(Guid Id, string Name);
    private sealed record TypeRecord(Guid Id, string Name);
}
