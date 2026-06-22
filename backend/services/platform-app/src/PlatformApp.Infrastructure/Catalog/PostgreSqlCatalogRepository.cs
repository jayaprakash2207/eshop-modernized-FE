using Dapper;
using PlatformApp.Application.Catalog;
using PlatformApp.Domain.Catalog;
using PlatformApp.Infrastructure.Persistence;

namespace PlatformApp.Infrastructure.Catalog;

public sealed class PostgreSqlCatalogRepository : ICatalogRepository
{
    private readonly PostgreSqlConnectionFactory _connectionFactory;

    public PostgreSqlCatalogRepository(PostgreSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyCollection<CatalogBrand>> ListBrandsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, name
            FROM catalog.catalog_brands
            ORDER BY name;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<BrandRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.Select(row => CatalogBrand.Restore(row.Id, row.Name)).ToArray();
    }

    public async Task<IReadOnlyCollection<CatalogType>> ListTypesAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, name
            FROM catalog.catalog_types
            ORDER BY name;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<TypeRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.Select(row => CatalogType.Restore(row.Id, row.Name)).ToArray();
    }

    public async Task<(IReadOnlyCollection<CatalogItem> Items, int TotalCount)> ListItemsAsync(CatalogItemsQuery query, CancellationToken cancellationToken)
    {
        const string countSql = """
            SELECT COUNT(*)
            FROM catalog.catalog_items
            WHERE (@CatalogBrandId IS NULL OR catalog_brand_id = @CatalogBrandId)
              AND (@CatalogTypeId IS NULL OR catalog_type_id = @CatalogTypeId);
            """;

        const string sql = """
            SELECT id, name, description, price, catalog_brand_id, catalog_type_id, picture_uri, available_stock
            FROM catalog.catalog_items
            WHERE (@CatalogBrandId IS NULL OR catalog_brand_id = @CatalogBrandId)
              AND (@CatalogTypeId IS NULL OR catalog_type_id = @CatalogTypeId)
            ORDER BY name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            query.CatalogBrandId,
            query.CatalogTypeId,
            Offset = query.PageIndex * query.PageSize,
            query.PageSize
        };

        await using var connection = _connectionFactory.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var rows = await connection.QueryAsync<ItemRow>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return (rows.Select(MapItem).ToArray(), totalCount);
    }

    public async Task<CatalogItem?> GetItemByIdAsync(Guid catalogItemId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, name, description, price, catalog_brand_id, catalog_type_id, picture_uri, available_stock
            FROM catalog.catalog_items
            WHERE id = @catalogItemId;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<ItemRow>(new CommandDefinition(sql, new { catalogItemId }, cancellationToken: cancellationToken));
        return row is null ? null : MapItem(row);
    }

    public async Task<CatalogItem> AddItemAsync(CatalogItem item, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO catalog.catalog_items
            (id, name, description, price, catalog_brand_id, catalog_type_id, picture_uri, available_stock, created_at, updated_at)
            VALUES
            (@Id, @Name, @Description, @Price, @CatalogBrandId, @CatalogTypeId, @PictureUri, @AvailableStock, NOW(), NOW());
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            item.Id,
            item.Name,
            item.Description,
            item.Price,
            item.CatalogBrandId,
            item.CatalogTypeId,
            item.PictureUri,
            item.AvailableStock
        }, cancellationToken: cancellationToken));

        return item;
    }

    public async Task<CatalogItem> UpdateItemAsync(CatalogItem item, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE catalog.catalog_items
            SET name = @Name,
                description = @Description,
                price = @Price,
                catalog_brand_id = @CatalogBrandId,
                catalog_type_id = @CatalogTypeId,
                picture_uri = @PictureUri,
                available_stock = @AvailableStock,
                updated_at = NOW()
            WHERE id = @Id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            item.Id,
            item.Name,
            item.Description,
            item.Price,
            item.CatalogBrandId,
            item.CatalogTypeId,
            item.PictureUri,
            item.AvailableStock
        }, cancellationToken: cancellationToken));

        return item;
    }

    public async Task<bool> DeleteItemAsync(Guid catalogItemId, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM catalog.catalog_items WHERE id = @catalogItemId;";
        await using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new { catalogItemId }, cancellationToken: cancellationToken));
        return affected > 0;
    }

    private static CatalogItem MapItem(ItemRow row)
    {
        return CatalogItem.Restore(row.Id, row.Name, row.Description, row.Price, row.CatalogBrandId, row.CatalogTypeId, row.PictureUri, row.AvailableStock);
    }

    private sealed record BrandRow(Guid Id, string Name);
    private sealed record TypeRow(Guid Id, string Name);
    private sealed record ItemRow(Guid Id, string Name, string Description, decimal Price, Guid CatalogBrandId, Guid CatalogTypeId, string PictureUri, int AvailableStock);
}
