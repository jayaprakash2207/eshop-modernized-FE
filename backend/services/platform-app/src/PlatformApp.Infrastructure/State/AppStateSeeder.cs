using PlatformApp.Domain.Catalog;
using PlatformApp.Infrastructure.Identity;

namespace PlatformApp.Infrastructure.State;

public static class AppStateSeeder
{
    public static AppState Seed()
    {
        var state = new AppState();

        var apparel = new CatalogType("Apparel");
        var bags = new CatalogType("Bags");
        var accessories = new CatalogType("Accessories");

        var contoso = new CatalogBrand("Contoso");
        var fabrikam = new CatalogBrand("Fabrikam");
        var adventure = new CatalogBrand("Adventure Works");

        state.Types.AddRange([apparel, bags, accessories]);
        state.Brands.AddRange([contoso, fabrikam, adventure]);
        state.CatalogItems.AddRange(
        [
            new CatalogItem("Trail Jacket", "Water-resistant technical jacket.", 129.00m, contoso.Id, apparel.Id, "/images/trail-jacket.png", 12),
            new CatalogItem("Commuter Tote", "Structured daily bag with laptop sleeve.", 89.00m, fabrikam.Id, bags.Id, "/images/commuter-tote.png", 8),
            new CatalogItem("Summit Bottle", "Insulated bottle for travel and hiking.", 34.50m, adventure.Id, accessories.Id, "/images/summit-bottle.png", 21),
            new CatalogItem("Weekend Duffel", "Carry-on duffel for short trips.", 109.00m, fabrikam.Id, bags.Id, "/images/weekend-duffel.png", 5),
            new CatalogItem("Base Layer Set", "Thermal base layer for cold weather.", 75.00m, contoso.Id, apparel.Id, "/images/base-layer.png", 17)
        ]);

        state.Users.AddRange(
        [
            new AppUser
            {
                Username = "admin",
                Password = PasswordHasher.HashPassword("Admin123!"),
                Email = "admin@eshop.local",
                PhoneNumber = "555-0100",
                Role = "Admin",
                EmailConfirmed = true
            },
            new AppUser
            {
                Username = "buyer",
                Password = PasswordHasher.HashPassword("Buyer123!"),
                Email = "buyer@eshop.local",
                PhoneNumber = "555-0110",
                Role = "User",
                EmailConfirmed = true
            }
        ]);

        // seed in-memory loyalty tiers and rules
        state.MembershipTiers.Add(new PlatformApp.Domain.Loyalty.MembershipTier { Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"), Name = "Bronze", MinPoints = 0, MaxPoints = 999 });
        state.MembershipTiers.Add(new PlatformApp.Domain.Loyalty.MembershipTier { Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"), Name = "Silver", MinPoints = 1000, MaxPoints = 4999 });
        state.MembershipTiers.Add(new PlatformApp.Domain.Loyalty.MembershipTier { Id = Guid.Parse("a0000000-0000-0000-0000-000000000003"), Name = "Gold", MinPoints = 5000 });
        state.RewardRules.Add(new PlatformApp.Domain.Loyalty.RewardRule { Id = Guid.Parse("b0000000-0000-0000-0000-000000000001"), Name = "Default Earn Rule", EarnMultiplier = 1.0m, MinOrderTotal = 0.00m, IsActive = true });

        return state;
    }
}
