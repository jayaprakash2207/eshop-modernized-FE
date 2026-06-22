using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PlatformApp.Integration.Tests.Infrastructure;
using Xunit;

namespace PlatformApp.Integration.Tests.Catalog;

public sealed class CatalogEndpointTests : IClassFixture<PlatformAppFactory>
{
    private readonly HttpClient _client;

    public CatalogEndpointTests(PlatformAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCatalogBrands_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/catalog-brands");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var brands = await response.Content.ReadFromJsonAsync<object[]>();
        brands.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCatalogTypes_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/catalog-types");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCatalogItems_WithDefaultPaging_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/catalog-items?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCatalogItemById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/catalog-items/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCatalogItem_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/catalog-items", new
        {
            name = "Test Item",
            description = "Test",
            price = 9.99,
            catalogBrandId = Guid.NewGuid(),
            catalogTypeId = Guid.NewGuid(),
            pictureUri = "",
            availableStock = 10
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Legacy route backward-compatibility
    [Fact]
    public async Task LegacyGetCatalogBrands_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/catalog-brands");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LegacyGetCatalogItems_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/catalog-items");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
