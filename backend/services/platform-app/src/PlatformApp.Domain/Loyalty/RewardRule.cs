namespace PlatformApp.Domain.Loyalty;

public sealed class RewardRule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal EarnMultiplier { get; set; } = 1.0m; // points per currency unit
    public decimal MinOrderTotal { get; set; } = 0.0m;
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}
