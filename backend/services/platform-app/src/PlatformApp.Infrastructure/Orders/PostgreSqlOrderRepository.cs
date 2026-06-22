using Dapper;
using PlatformApp.Application.Orders;
using PlatformApp.Domain.Orders;
using PlatformApp.Infrastructure.Persistence;

namespace PlatformApp.Infrastructure.Orders;

public sealed class PostgreSqlOrderRepository : IOrderRepository
{
    private readonly PostgreSqlConnectionFactory _connectionFactory;

    public PostgreSqlOrderRepository(PostgreSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task SaveAsync(Order order, CancellationToken cancellationToken)
    {
        const string orderSql = """
            INSERT INTO orders.orders
            (id, buyer_id, order_number, status, total, shipping_address_line1, shipping_address_city, shipping_address_state, shipping_address_postal_code, shipping_address_country, created_at, updated_at)
            VALUES
            (@Id, @BuyerId, @OrderNumber, @Status, @Total, @Street, @City, @State, @PostalCode, @Country, NOW(), NOW());
            """;

        const string itemSql = """
            INSERT INTO orders.order_items
            (id, order_id, catalog_item_id, product_name, unit_price, units, created_at, updated_at)
            VALUES
            (@Id, @OrderId, @CatalogItemId, @ProductName, @UnitPrice, @Units, NOW(), NOW());
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(orderSql, new
        {
            order.Id,
            order.BuyerId,
            order.OrderNumber,
            order.Status,
            order.Total,
            Street = order.ShippingAddress.Street,
            City = order.ShippingAddress.City,
            State = order.ShippingAddress.State,
            PostalCode = order.ShippingAddress.PostalCode,
            Country = order.ShippingAddress.Country
        }, transaction, cancellationToken: cancellationToken));

        foreach (var item in order.Items)
        {
            await connection.ExecuteAsync(new CommandDefinition(itemSql, new
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                item.CatalogItemId,
                item.ProductName,
                item.UnitPrice,
                item.Units
            }, transaction, cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> ListByBuyerAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id
            FROM orders.orders
            WHERE buyer_id = @buyerId
            ORDER BY created_at DESC;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        var orderIds = await connection.QueryAsync<Guid>(new CommandDefinition(sql, new { buyerId }, cancellationToken: cancellationToken));
        var orders = new List<Order>();

        foreach (var orderId in orderIds)
        {
            var order = await GetByIdAsync(buyerId, orderId, cancellationToken);
            if (order is not null)
            {
                orders.Add(order);
            }
        }

        return orders;
    }

    public async Task<Order?> GetByIdAsync(Guid buyerId, Guid orderId, CancellationToken cancellationToken)
    {
        const string orderSql = """
            SELECT id, buyer_id, order_number, status, total, shipping_address_line1, shipping_address_city, shipping_address_state, shipping_address_postal_code, shipping_address_country, created_at
            FROM orders.orders
            WHERE id = @orderId AND buyer_id = @buyerId;
            """;

        const string itemSql = """
            SELECT catalog_item_id, product_name, unit_price, units
            FROM orders.order_items
            WHERE order_id = @orderId;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        var orderRow = await connection.QuerySingleOrDefaultAsync<OrderRow>(new CommandDefinition(orderSql, new { orderId, buyerId }, cancellationToken: cancellationToken));
        if (orderRow is null)
        {
            return null;
        }

        var items = await connection.QueryAsync<OrderItemRow>(new CommandDefinition(itemSql, new { orderId }, cancellationToken: cancellationToken));
        return Order.Restore(
            orderRow.Id,
            orderRow.BuyerId,
            "buyer",
            orderRow.OrderNumber,
            orderRow.Status,
            new Address(orderRow.Street, orderRow.City, orderRow.State, orderRow.PostalCode, orderRow.Country),
            items.Select(item => new OrderItem(item.CatalogItemId, item.ProductName, item.UnitPrice, item.Units)),
            orderRow.CreatedAt);
    }

    private sealed record OrderRow(Guid Id, Guid BuyerId, string OrderNumber, string Status, decimal Total, string Street, string City, string State, string PostalCode, string Country, DateTimeOffset CreatedAt);
    private sealed record OrderItemRow(Guid CatalogItemId, string ProductName, decimal UnitPrice, int Units);
}
