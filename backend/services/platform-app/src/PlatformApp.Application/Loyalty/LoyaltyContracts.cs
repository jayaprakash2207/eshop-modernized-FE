namespace PlatformApp.Application.Loyalty;

public sealed record EarnPointsRequest(Guid OrderId, decimal OrderTotal, string? SourceEventId = null);
public sealed record RedeemPointsRequest(Guid AccountId, long Points);
public sealed record LoyaltyBalanceDto(long PointsBalance, string Tier);
public sealed record LoyaltyTransactionDto(Guid Id, string Type, long Points, DateTimeOffset CreatedAtUtc, DateTimeOffset? ExpiresAtUtc, Guid? OrderId);

public sealed record RewardRuleDto(Guid Id, string Name, decimal EarnMultiplier, decimal MinOrderTotal, bool IsActive);

public interface ILoyaltyService
{
    Task<LoyaltyBalanceDto> GetBalanceAsync(Guid userId, CancellationToken cancellationToken);
    Task<IEnumerable<LoyaltyTransactionDto>> GetHistoryAsync(Guid userId, int pageIndex, int pageSize, CancellationToken cancellationToken);
    Task<long> EarnPointsAsync(Guid userId, EarnPointsRequest request, CancellationToken cancellationToken);
    Task RedeemPointsAsync(Guid userId, RedeemPointsRequest request, CancellationToken cancellationToken);

    // Admin
    Task<IEnumerable<RewardRuleDto>> GetRewardRulesAsync(CancellationToken cancellationToken = default);
    Task<RewardRuleDto> CreateRewardRuleAsync(RewardRuleDto dto, CancellationToken cancellationToken = default);
    Task UpdateRewardRuleAsync(RewardRuleDto dto, CancellationToken cancellationToken = default);
    Task DeleteRewardRuleAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ILoyaltyRepository
{
    Task<Domain.Loyalty.LoyaltyAccount?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Domain.Loyalty.LoyaltyAccount> CreateAccountAsync(Guid userId, CancellationToken cancellationToken);
    Task<long> GetBalanceAsync(Guid accountId, CancellationToken cancellationToken);
    Task<IEnumerable<Domain.Loyalty.LoyaltyTransaction>> GetTransactionsAsync(Guid accountId, int pageIndex, int pageSize, CancellationToken cancellationToken);
    Task CreditAsync(Domain.Loyalty.LoyaltyTransaction transaction, CancellationToken cancellationToken);
    Task DebitAsync(Domain.Loyalty.LoyaltyTransaction transaction, CancellationToken cancellationToken);
    Task<Domain.Loyalty.MembershipTier?> GetTierByPointsAsync(long points, CancellationToken cancellationToken);
    Task<IEnumerable<Domain.Loyalty.MembershipTier>> ListTiersAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Domain.Loyalty.RewardRule>> ListActiveRulesAsync(CancellationToken cancellationToken);
    Task CreateRuleAsync(Domain.Loyalty.RewardRule rule, CancellationToken cancellationToken);
    Task UpdateRuleAsync(Domain.Loyalty.RewardRule rule, CancellationToken cancellationToken);
    Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken);
    Task ExpirePointsAsync(DateTimeOffset now, CancellationToken cancellationToken);
}
