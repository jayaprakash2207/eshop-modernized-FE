using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PlatformApp.Integration.Tests.Infrastructure;
using Xunit;

namespace PlatformApp.Integration.Tests.Identity;

public sealed class AuthEndpointTests : IClassFixture<PlatformAppFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(PlatformAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Authenticate_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/authenticate", new
        {
            username = "nonexistent@example.com",
            password = "WrongPassword1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/v1/Account/Register", new
        {
            username = uniqueEmail,
            email = uniqueEmail,
            password = "SecurePass1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_ThenAuthenticate_ReturnsToken()
    {
        var email = $"auth-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass1!";

        var registerResp = await _client.PostAsJsonAsync("/api/v1/Account/Register", new
        {
            username = email, email, password
        });
        registerResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var authResp = await _client.PostAsJsonAsync("/api/v1/authenticate", new
        {
            username = email, password
        });
        authResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await authResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body.Should().ContainKey("accessToken");
    }

    [Fact]
    public async Task LegacyAuthenticate_ReturnsOkOrUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/authenticate", new
        {
            username = "nobody@example.com",
            password = "wrongpass"
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }
}
