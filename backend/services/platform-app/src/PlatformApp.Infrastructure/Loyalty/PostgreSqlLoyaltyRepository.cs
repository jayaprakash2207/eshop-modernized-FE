using Dapper;
using PlatformApp.Infrastructure.Persistence;
using PlatformApp.Domain.Loyalty;

namespace PlatformApp.Infrastructure.Loyalty;

public sealed class PostgreSqlLoyaltyRepository : PlatformApp.Application.Loyalty.ILoyaltyRepository
{
    private readonly PostgreSqlConnectionFactory _connectionFactory;

    public PostgreSqlLoyaltyRepository(PostgreSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Domain.Loyalty.LoyaltyAccount?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, user_id, points_balance, tier_id, created_at, updated_at FROM loyalty.loyalty_accounts WHERE user_id = @UserId LIMIT 1;";
        await using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<AccountRow>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        if (row is null) return null;
        var acc = new Domain.Loyalty.LoyaltyAccount(row.Id, row.UserId);
        // set points balance via reflection/assignment using private API not available; use workaround by creating new instance
        // We'll set via internal operations: create a new aggregate and credit points accordingly
        if (row.PointsBalance > 0) acc.CreditPoints(row.PointsBalance);
        return acc;
    }

    public async Task<Domain.Loyalty.LoyaltyAccount> CreateAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        const string sql = "INSERT INTO loyalty.loyalty_accounts (id, user_id, points_balance, created_at, updated_at) VALUES (@Id, @UserId, 0, NOW(), NOW()) ON CONFLICT DO NOTHING;";
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, UserId = userId }, cancellationToken: cancellationToken));
        return new Domain.Loyalty.LoyaltyAccount(id, userId);
    }

    public async Task<long> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT points_balance FROM loyalty.loyalty_accounts WHERE id = @AccountId;";
        await using var connection = _connectionFactory.CreateConnection();
        var balance = await connection.ExecuteScalarAsync<long?>(new CommandDefinition(sql, new { AccountId = accountId }, cancellationToken: cancellationToken));
        return balance ?? 0L;
    }

    public async Task<IEnumerable<Domain.Loyalty.LoyaltyTransaction>> GetTransactionsAsync(Guid accountId, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, account_id, type, points, order_id, source_event_id, created_at, expires_at FROM loyalty.loyalty_transactions WHERE account_id = @AccountId ORDER BY created_at DESC OFFSET @Offset LIMIT @Limit;";
        await using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<TransactionRow>(new CommandDefinition(sql, new { AccountId = accountId, Offset = pageIndex * pageSize, Limit = pageSize }, cancellationToken: cancellationToken));
        return rows.Select(r => new Domain.Loyalty.LoyaltyTransaction(r.Id, r.AccountId, r.Type, r.Points, r.OrderId, r.SourceEventId, r.ExpiresAt));
    }

    public async Task CreditAsync(Domain.Loyalty.LoyaltyTransaction transaction, CancellationToken cancellationToken)
    {
        const string insertTx = "INSERT INTO loyalty.loyalty_transactions (id, account_id, type, points, order_id, source_event_id, created_at, expires_at) VALUES (@Id, @AccountId, @Type, @Points, @OrderId, @SourceEventId, NOW(), @ExpiresAt);";
        const string updateAccount = "UPDATE loyalty.loyalty_accounts SET points_balance = points_balance + @Points, updated_at = NOW() WHERE id = @AccountId;";
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(insertTx, new { transaction.Id, transaction.AccountId, transaction.Type, transaction.Points, transaction.OrderId, transaction.SourceEventId, ExpiresAt = transaction.ExpiresAtUtc }, tx, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(updateAccount, new { Points = transaction.Points, AccountId = transaction.AccountId }, tx, cancellationToken: cancellationToken));
        await tx.CommitAsync(cancellationToken);
    }

    public async Task DebitAsync(Domain.Loyalty.LoyaltyTransaction transaction, CancellationToken cancellationToken)
    {
        const string insertTx = "INSERT INTO loyalty.loyalty_transactions (id, account_id, type, points, order_id, source_event_id, created_at) VALUES (@Id, @AccountId, @Type, @Points, @OrderId, @SourceEventId, NOW());";
        const string updateAccount = "UPDATE loyalty.loyalty_accounts SET points_balance = points_balance + @Points, updated_at = NOW() WHERE id = @AccountId;"; // Points negative for debit
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(insertTx, new { transaction.Id, transaction.AccountId, transaction.Type, transaction.Points, transaction.OrderId, transaction.SourceEventId }, tx, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(updateAccount, new { Points = transaction.Points, AccountId = transaction.AccountId }, tx, cancellationToken: cancellationToken));
        await tx.CommitAsync(cancellationToken);
    }

    public async Task<Domain.Loyalty.MembershipTier?> GetTierByPointsAsync(long points, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, min_points, max_points FROM loyalty.membership_tiers WHERE min_points <= @Points ORDER BY min_points DESC LIMIT 1;";
        await using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<TierRow>(new CommandDefinition(sql, new { Points = points }, cancellationToken: cancellationToken));
        return row is null ? null : new Domain.Loyalty.MembershipTier { Id = row.Id, Name = row.Name, MinPoints = row.MinPoints, MaxPoints = row.MaxPoints };
    }

    public async Task<IEnumerable<Domain.Loyalty.MembershipTier>> ListTiersAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, min_points, max_points FROM loyalty.membership_tiers ORDER BY min_points;";
        await using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<TierRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.Select(r => new Domain.Loyalty.MembershipTier { Id = r.Id, Name = r.Name, MinPoints = r.MinPoints, MaxPoints = r.MaxPoints });
    }

    public async Task<IEnumerable<Domain.Loyalty.RewardRule>> ListActiveRulesAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, earn_multiplier, min_order_total, valid_from, valid_to, is_active FROM loyalty.reward_rules WHERE is_active = TRUE ORDER BY earn_multiplier DESC;";
        await using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<RuleRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.Select(r => new Domain.Loyalty.RewardRule { Id = r.Id, Name = r.Name, EarnMultiplier = r.EarnMultiplier, MinOrderTotal = r.MinOrderTotal, ValidFrom = r.ValidFrom, ValidTo = r.ValidTo, IsActive = r.IsActive });
    }

    public async Task CreateRuleAsync(Domain.Loyalty.RewardRule rule, CancellationToken cancellationToken)
    {
        const string sql = "INSERT INTO loyalty.reward_rules (id, name, earn_multiplier, min_order_total, valid_from, valid_to, is_active, created_at) VALUES (@Id, @Name, @EarnMultiplier, @MinOrderTotal, @ValidFrom, @ValidTo, @IsActive, NOW());";
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { rule.Id, rule.Name, rule.EarnMultiplier, rule.MinOrderTotal, rule.ValidFrom, rule.ValidTo, rule.IsActive }, cancellationToken: cancellationToken));
    }

    public async Task UpdateRuleAsync(Domain.Loyalty.RewardRule rule, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE loyalty.reward_rules SET name = @Name, earn_multiplier = @EarnMultiplier, min_order_total = @MinOrderTotal, valid_from = @ValidFrom, valid_to = @ValidTo, is_active = @IsActive, updated_at = NOW() WHERE id = @Id;";
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { rule.Id, rule.Name, rule.EarnMultiplier, rule.MinOrderTotal, rule.ValidFrom, rule.ValidTo, rule.IsActive }, cancellationToken: cancellationToken));
    }

    public async Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM loyalty.reward_rules WHERE id = @Id;";
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task ExpirePointsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        // Find all earning transactions that have expired and have not yet been reversed.
        const string findSql = @"SELECT t.id, t.account_id, t.points FROM loyalty.loyalty_transactions t
WHERE t.type = 'EARN' AND t.expires_at IS NOT NULL AND t.expires_at <= @Now
AND NOT EXISTS(SELECT 1 FROM loyalty.loyalty_transactions r WHERE r.source_event_id = t.source_event_id AND r.type = 'REDEEM' AND r.points < 0);
";

        const string insertReversal = "INSERT INTO loyalty.loyalty_transactions (id, account_id, type, points, order_id, source_event_id, created_at) VALUES (@Id, @AccountId, 'REDEEM', @Points, NULL, @SourceEventId, NOW());";
        const string updateAccount = "UPDATE loyalty.loyalty_accounts SET points_balance = points_balance + @Points, updated_at = NOW() WHERE id = @AccountId;";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<TransactionToExpire>(new CommandDefinition(findSql, new { Now = now }, cancellationToken: cancellationToken));
        foreach (var row in rows)
        {
            var reversalId = Guid.NewGuid();
            var reversalPoints = -Math.Abs(row.Points);
            await using var tx = await connection.BeginTransactionAsync(cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition(insertReversal, new { Id = reversalId, AccountId = row.AccountId, Points = reversalPoints, SourceEventId = row.Id }, tx, cancellationToken: cancellationToken));
            await connection.ExecuteAsync(new CommandDefinition(updateAccount, new { Points = reversalPoints, AccountId = row.AccountId }, tx, cancellationToken: cancellationToken));
            await tx.CommitAsync(cancellationToken);
        }
    }

    private sealed record TransactionToExpire(Guid Id, Guid AccountId, long Points);

    private sealed record AccountRow(Guid Id, Guid UserId, long PointsBalance, Guid? TierId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
    private sealed record TransactionRow(Guid Id, Guid AccountId, string Type, long Points, Guid? OrderId, string? SourceEventId, DateTimeOffset CreatedAt, DateTimeOffset? ExpiresAt);
    private sealed record TierRow(Guid Id, string Name, long MinPoints, long? MaxPoints);
    private sealed record RuleRow(Guid Id, string Name, decimal EarnMultiplier, decimal MinOrderTotal, DateTimeOffset? ValidFrom, DateTimeOffset? ValidTo, bool IsActive);
}
