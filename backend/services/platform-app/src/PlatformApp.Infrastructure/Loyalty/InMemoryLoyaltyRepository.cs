using PlatformApp.Application.Loyalty;
using PlatformApp.Domain.Loyalty;
using PlatformApp.Infrastructure.State;

namespace PlatformApp.Infrastructure.Loyalty;

public sealed class InMemoryLoyaltyRepository : ILoyaltyRepository
{
    private readonly AppState _state;

    public InMemoryLoyaltyRepository(AppState state)
    {
        _state = state;
    }

    public Task<Domain.Loyalty.LoyaltyAccount?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var acc = _state.LoyaltyAccounts.FirstOrDefault(a => a.UserId == userId);
        return Task.FromResult(acc);
    }

    public Task<Domain.Loyalty.LoyaltyAccount> CreateAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var acc = new Domain.Loyalty.LoyaltyAccount(Guid.NewGuid(), userId);
        _state.LoyaltyAccounts.Add(acc);
        return Task.FromResult(acc);
    }

    public Task<long> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var acc = _state.LoyaltyAccounts.FirstOrDefault(a => a.Id == accountId);
        return Task.FromResult(acc?.PointsBalance ?? 0L);
    }

    public Task<IEnumerable<Domain.Loyalty.LoyaltyTransaction>> GetTransactionsAsync(Guid accountId, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var txs = _state.LoyaltyTransactions.Where(t => t.AccountId == accountId).OrderByDescending(t => t.CreatedAtUtc).Skip(pageIndex * pageSize).Take(pageSize).ToArray();
        return Task.FromResult<IEnumerable<Domain.Loyalty.LoyaltyTransaction>>(txs);
    }

    public Task CreditAsync(Domain.Loyalty.LoyaltyTransaction transaction, CancellationToken cancellationToken)
    {
        var acc = _state.LoyaltyAccounts.First(a => a.Id == transaction.AccountId);
        acc.CreditPoints(transaction.Points);
        _state.LoyaltyTransactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task DebitAsync(Domain.Loyalty.LoyaltyTransaction transaction, CancellationToken cancellationToken)
    {
        var acc = _state.LoyaltyAccounts.First(a => a.Id == transaction.AccountId);
        acc.DebitPoints(Math.Abs(transaction.Points));
        _state.LoyaltyTransactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task<Domain.Loyalty.MembershipTier?> GetTierByPointsAsync(long points, CancellationToken cancellationToken)
    {
        var tier = _state.MembershipTiers.Where(t => t.MinPoints <= points).OrderByDescending(t => t.MinPoints).FirstOrDefault();
        return Task.FromResult(tier);
    }

    public Task<IEnumerable<Domain.Loyalty.MembershipTier>> ListTiersAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<Domain.Loyalty.MembershipTier>>(_state.MembershipTiers.OrderBy(t => t.MinPoints).ToArray());
    }

    public Task<IEnumerable<Domain.Loyalty.RewardRule>> ListActiveRulesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<Domain.Loyalty.RewardRule>>(_state.RewardRules.Where(r => r.IsActive).OrderByDescending(r => r.EarnMultiplier).ToArray());
    }

    public Task CreateRuleAsync(Domain.Loyalty.RewardRule rule, CancellationToken cancellationToken)
    {
        _state.RewardRules.Add(rule);
        return Task.CompletedTask;
    }

    public Task UpdateRuleAsync(Domain.Loyalty.RewardRule rule, CancellationToken cancellationToken)
    {
        var idx = _state.RewardRules.FindIndex(r => r.Id == rule.Id);
        if (idx >= 0) _state.RewardRules[idx] = rule;
        return Task.CompletedTask;
    }

    public Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken)
    {
        _state.RewardRules.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }

    public Task ExpirePointsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var toExpire = _state.LoyaltyTransactions.Where(t => t.ExpiresAtUtc.HasValue && t.ExpiresAtUtc.Value <= now && t.Type == "EARN").ToArray();
        foreach (var t in toExpire)
        {
            if (_state.LoyaltyTransactions.Any(r => r.SourceEventId == t.SourceEventId && r.Type == "REDEEM" && r.Points < 0)) continue;
            var reversal = new Domain.Loyalty.LoyaltyTransaction(Guid.NewGuid(), t.AccountId, "REDEEM", -Math.Abs(t.Points), null, t.Id.ToString(), null);
            var acc = _state.LoyaltyAccounts.First(a => a.Id == t.AccountId);
            acc.DebitPoints(Math.Abs(reversal.Points));
            _state.LoyaltyTransactions.Add(reversal);
        }

        return Task.CompletedTask;
    }
}
