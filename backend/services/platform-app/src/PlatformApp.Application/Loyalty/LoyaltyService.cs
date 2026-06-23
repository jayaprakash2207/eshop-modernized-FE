using PlatformApp.Application.Abstractions;
using PlatformApp.Domain.Loyalty;

namespace PlatformApp.Application.Loyalty;

public sealed class LoyaltyService : ILoyaltyService
{
    private readonly ILoyaltyRepository _repository;
    private readonly IDomainEventPublisher _publisher;

    public LoyaltyService(ILoyaltyRepository repository, IDomainEventPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<LoyaltyBalanceDto> GetBalanceAsync(Guid userId, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByUserIdAsync(userId, cancellationToken) ?? await _repository.CreateAccountAsync(userId, cancellationToken);
        var tier = await _repository.GetTierByPointsAsync(account.PointsBalance, cancellationToken);
        return new LoyaltyBalanceDto(account.PointsBalance, tier?.Name ?? "Bronze");
    }

    public async Task<IEnumerable<LoyaltyTransactionDto>> GetHistoryAsync(Guid userId, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByUserIdAsync(userId, cancellationToken) ?? await _repository.CreateAccountAsync(userId, cancellationToken);
        var txs = await _repository.GetTransactionsAsync(account.Id, pageIndex, pageSize, cancellationToken);
        return txs.Select(t => new LoyaltyTransactionDto(t.Id, t.Type, t.Points, t.CreatedAtUtc, t.ExpiresAtUtc, t.OrderId));
    }

    public async Task<long> EarnPointsAsync(Guid userId, EarnPointsRequest request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByUserIdAsync(userId, cancellationToken) ?? await _repository.CreateAccountAsync(userId, cancellationToken);
        var rules = await _repository.ListActiveRulesAsync(cancellationToken);
        var rule = rules.OrderByDescending(r => r.EarnMultiplier).FirstOrDefault();
        var multiplier = rule?.EarnMultiplier ?? 1.0m;
        var points = (long)Math.Floor(request.OrderTotal * multiplier);

        var tx = new Domain.Loyalty.LoyaltyTransaction(Guid.NewGuid(), account.Id, "EARN", points, request.OrderId, request.SourceEventId, null);
        await _repository.CreditAsync(tx, cancellationToken);
        await _publisher.PublishAsync(new PointsEarnedEvent(account.Id, points, request.OrderId, request.SourceEventId), cancellationToken);
        return points;
    }

    public async Task RedeemPointsAsync(Guid userId, RedeemPointsRequest request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByUserIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("Loyalty account not found.");
        if (request.Points > account.PointsBalance) throw new InvalidOperationException("Insufficient points.");

        var tx = new Domain.Loyalty.LoyaltyTransaction(Guid.NewGuid(), account.Id, "REDEEM", -Math.Abs(request.Points), null, null, null);
        await _repository.DebitAsync(tx, cancellationToken);
        await _publisher.PublishAsync(new PointsRedeemedEvent(account.Id, -Math.Abs(request.Points), null, null), cancellationToken);
    }

    // Admin operations
    public async Task<IEnumerable<RewardRuleDto>> GetRewardRulesAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _repository.ListActiveRulesAsync(cancellationToken);
        return rules.Select(r => new RewardRuleDto(r.Id, r.Name, r.EarnMultiplier, r.MinOrderTotal, r.IsActive));
    }

    public async Task<RewardRuleDto> CreateRewardRuleAsync(RewardRuleDto dto, CancellationToken cancellationToken = default)
    {
        var rule = new Domain.Loyalty.RewardRule(dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id, dto.Name, dto.EarnMultiplier, dto.MinOrderTotal, dto.IsActive);
        await _repository.CreateRuleAsync(rule, cancellationToken);
        return new RewardRuleDto(rule.Id, rule.Name, rule.EarnMultiplier, rule.MinOrderTotal, rule.IsActive);
    }

    public async Task UpdateRewardRuleAsync(RewardRuleDto dto, CancellationToken cancellationToken = default)
    {
        var rule = new Domain.Loyalty.RewardRule(dto.Id, dto.Name, dto.EarnMultiplier, dto.MinOrderTotal, dto.IsActive);
        await _repository.UpdateRuleAsync(rule, cancellationToken);
    }

    public async Task DeleteRewardRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteRuleAsync(id, cancellationToken);
    }
}
