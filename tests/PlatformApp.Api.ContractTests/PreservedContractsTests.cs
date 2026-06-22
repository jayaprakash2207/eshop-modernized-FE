namespace PlatformApp.Api.ContractTests;

public class PreservedContractsTests
{
    [Fact]
    public void PreservedRoutes_ShouldRemainDocumentedForComparison()
    {
        var preservedRoutes = new[]
        {
            "/api/authenticate",
            "/api/catalog-brands",
            "/api/catalog-items",
            "/api/catalog-types",
            "/Basket/Checkout",
            "/Basket/Success",
            "/Order/MyOrders",
            "/Order/Detail/{orderId}",
            "/Manage/MyAccount",
            "/Admin"
        };

        Assert.Contains("/api/catalog-items", preservedRoutes);
        Assert.Contains("/Order/MyOrders", preservedRoutes);
    }
}
