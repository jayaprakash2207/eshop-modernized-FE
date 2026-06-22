namespace PlatformApp.Domain.Loyalty;

public sealed class MembershipTier
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long MinPoints { get; set; }
    public long? MaxPoints { get; set; }
}
