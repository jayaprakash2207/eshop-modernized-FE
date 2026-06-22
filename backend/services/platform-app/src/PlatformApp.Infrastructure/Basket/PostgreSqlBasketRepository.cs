using Dapper;
using PlatformApp.Application.Basket;
using PlatformApp.Infrastructure.Persistence;

namespace PlatformApp.Infrastructure.Basket;

public sealed class PostgreSqlBasketRepository : IBasketRepository
{
    private readonly PostgreSqlConnectionFactory _connectionFactory;

    public PostgreSqlBasketRepository(PostgreSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Domain.Basket.Basket> GetOrCreateAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        const string basketSql = """
            SELECT id, buyer_id
            FROM basket.baskets
            WHERE buyer_id = @buyerId;
            """;

        const string itemsSql = """
            SELECT catalog_item_id, product_name, unit_price, quantity
            FROM basket.basket_items
            WHERE basket_id = @basketId;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        var basketRow = await connection.QuerySingleOrDefaultAsync<BasketRow>(new CommandDefinition(basketSql, new { buyerId }, cancellationToken: cancellationToken));

        if (basketRow is null)
        {
            var basket = new Domain.Basket.Basket(buyerId);
            await SaveAsync(basket, cancellationToken);
            return basket;
        }

        var basketAggregate = new Domain.Basket.Basket(buyerId);
        var itemRows = await connection.QueryAsync<BasketItemRow>(new CommandDefinition(itemsSql, new { basketId = basketRow.Id }, cancellationToken: cancellationToken));
        foreach (var item in itemRows)
        {
            basketAggregate.AddItem(item.CatalogItemId, item.ProductName, item.UnitPrice, item.Quantity);
        }

        return basketAggregate;
    }

    public async Task SaveAsync(Domain.Basket.Basket basket, CancellationToken cancellationToken)
    {
        const string upsertBasketSql = """
            INSERT INTO basket.baskets (id, buyer_id, created_at, updated_at)
            VALUES (@Id, @BuyerId, NOW(), NOW())
            ON CONFLICT (id) DO UPDATE SET updated_at = NOW(), buyer_id = EXCLUDED.buyer_id;
            """;

        const string selectBasketIdSql = "SELECT id FROM basket.baskets WHERE buyer_id = @BuyerId LIMIT 1;";
        const string deleteItemsSql = "DELETE FROM basket.basket_items WHERE basket_id = @BasketId;";
        const string insertItemSql = """
            INSERT INTO basket.basket_items
            (id, basket_id, catalog_item_id, product_name, unit_price, quantity, created_at, updated_at)
            VALUES
            (@Id, @BasketId, @CatalogItemId, @ProductName, @UnitPrice, @Quantity, NOW(), NOW());
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var basketId = await connection.ExecuteScalarAsync<Guid?>(
            new CommandDefinition(selectBasketIdSql, new { basket.BuyerId }, transaction, cancellationToken: cancellationToken));

        basketId ??= basket.Id;

        await connection.ExecuteAsync(new CommandDefinition(upsertBasketSql, new { Id = basketId, basket.BuyerId }, transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(deleteItemsSql, new { BasketId = basketId }, transaction, cancellationToken: cancellationToken));

        foreach (var item in basket.Items)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertItemSql, new
            {
                Id = Guid.NewGuid(),
                BasketId = basketId,
                item.CatalogItemId,
                item.ProductName,
                item.UnitPrice,
                item.Quantity
            }, transaction, cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private sealed record BasketRow(Guid Id, Guid BuyerId);
    private sealed record BasketItemRow(Guid CatalogItemId, string ProductName, decimal UnitPrice, int Quantity);
}
