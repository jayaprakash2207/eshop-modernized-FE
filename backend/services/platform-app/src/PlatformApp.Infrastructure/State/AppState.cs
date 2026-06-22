using PlatformApp.Domain.Basket;
using PlatformApp.Domain.Catalog;
using PlatformApp.Domain.Orders;

namespace PlatformApp.Infrastructure.State;

public sealed class AppState
{
    public List<CatalogBrand> Brands { get; } = [];
    public List<CatalogType> Types { get; } = [];
    public List<CatalogItem> CatalogItems { get; } = [];
    public Dictionary<Guid, Domain.Basket.Basket> Baskets { get; } = [];
    public List<Order> Orders { get; } = [];
    public List<AppUser> Users { get; } = [];
    public List<PlatformApp.Domain.Loyalty.LoyaltyAccount> LoyaltyAccounts { get; } = [];
    public List<PlatformApp.Domain.Loyalty.LoyaltyTransaction> LoyaltyTransactions { get; } = [];
    public List<PlatformApp.Domain.Loyalty.MembershipTier> MembershipTiers { get; } = [];
    public List<PlatformApp.Domain.Loyalty.RewardRule> RewardRules { get; } = [];
}

public sealed class AppUser
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool EmailConfirmed { get; set; }
}
