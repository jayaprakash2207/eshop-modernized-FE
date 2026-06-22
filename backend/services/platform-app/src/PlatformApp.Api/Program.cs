using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Observability;
using PlatformApp.Application.Basket;
using PlatformApp.Application.Catalog;
using PlatformApp.Application.Identity;
using PlatformApp.Application.Orders;
using PlatformApp.Application.Payments;
using PlatformApp.Infrastructure;
using PlatformApp.Infrastructure.Identity;
using BuildingBlocks.Security;
using Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUserAccessor, HttpCurrentUserAccessor>();
builder.Services.AddPlatformTelemetry("PlatformApp.Api");
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PlatformApp.Application.Loyalty.ILoyaltyService, PlatformApp.Application.Loyalty.LoyaltyService>();
builder.Services.AddHostedService<PlatformApp.Infrastructure.LoyaltyExpiryService>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(signingKey),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/api_health_check");
app.MapHealthChecks("/home_page_health_check");

app.MapPost("/api/authenticate", async (
    [FromBody] AuthenticateRequest request,
    IIdentityService identityService,
    CancellationToken cancellationToken) =>
{
    var response = await identityService.AuthenticateAsync(request, cancellationToken);
    return response is null ? Results.Unauthorized() : Results.Ok(response);
});

app.MapPost("/Account/Register", async (
    [FromBody] RegisterRequest request,
    IIdentityAccountService accountService,
    CancellationToken cancellationToken) =>
{
    var response = await accountService.RegisterAsync(request, cancellationToken);
    return Results.Created($"/User/{response.UserId}", response);
});

app.MapGet("/Account/Login", () => Results.Ok(new AccountStatusResponse("Use POST /api/authenticate to sign in.")));
app.MapGet("/Account/Logout", () => Results.Ok(new AccountStatusResponse("Use POST /User/Logout to sign out.")));
app.MapGet("/Account/ConfirmEmail", async (
    [FromQuery] string username,
    IIdentityAccountService accountService,
    CancellationToken cancellationToken) =>
{
    var response = await accountService.ConfirmEmailAsync(username, cancellationToken);
    return Results.Ok(response);
});

app.MapGet("/api/catalog-brands", async (CatalogService catalogService, CancellationToken cancellationToken) =>
{
    var brands = await catalogService.GetBrandsAsync(cancellationToken);
    return Results.Ok(brands);
});

app.MapGet("/api/catalog-types", async (CatalogService catalogService, CancellationToken cancellationToken) =>
{
    var types = await catalogService.GetTypesAsync(cancellationToken);
    return Results.Ok(types);
});

app.MapGet("/api/catalog-items", async (
    [AsParameters] CatalogItemsQuery query,
    CatalogService catalogService,
    CancellationToken cancellationToken) =>
{
    var response = await catalogService.GetItemsAsync(query, cancellationToken);
    return Results.Ok(response);
});

app.MapGet("/api/catalog-items/{catalogItemId:guid}", async (
    Guid catalogItemId,
    CatalogService catalogService,
    CancellationToken cancellationToken) =>
{
    var item = await catalogService.GetItemAsync(catalogItemId, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/catalog-items", async (
    [FromBody] UpsertCatalogItemRequest request,
    CatalogService catalogService,
    CancellationToken cancellationToken) =>
{
    var item = await catalogService.CreateItemAsync(request, cancellationToken);
    return Results.Created($"/api/catalog-items/{item.Id}", new { item.Id });
}).RequireAuthorization("AdminOnly");

app.MapPut("/api/catalog-items/{catalogItemId:guid}", async (
    Guid catalogItemId,
    [FromBody] UpsertCatalogItemRequest request,
    CatalogService catalogService,
    CancellationToken cancellationToken) =>
{
    var item = await catalogService.UpdateItemAsync(catalogItemId, request, cancellationToken);
    return item is null ? Results.NotFound() : Results.NoContent();
}).RequireAuthorization("AdminOnly");

app.MapPut("/api/catalog-items", async (
    [FromBody] UpdateCatalogItemRequest request,
    CatalogService catalogService,
    CancellationToken cancellationToken) =>
{
    var updateRequest = new UpsertCatalogItemRequest(
        request.Name,
        request.Description,
        request.Price,
        request.CatalogBrandId,
        request.CatalogTypeId,
        request.PictureUri,
        request.AvailableStock);

    var item = await catalogService.UpdateItemAsync(request.CatalogItemId, updateRequest, cancellationToken);
    return item is null ? Results.NotFound() : Results.NoContent();
}).RequireAuthorization("AdminOnly");

app.MapDelete("/api/catalog-items/{catalogItemId:guid}", async (
    Guid catalogItemId,
    CatalogService catalogService,
    CancellationToken cancellationToken) =>
{
    var deleted = await catalogService.DeleteItemAsync(catalogItemId, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization("AdminOnly");

app.MapGet("/User", async (
    ClaimsPrincipal user,
    IIdentityAccountService accountService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var profile = await accountService.GetProfileAsync(userId.Value, cancellationToken);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
}).RequireAuthorization();

app.MapPost("/User/Logout", async (
    ClaimsPrincipal user,
    IIdentityAccountService accountService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var response = await accountService.LogoutAsync(userId.Value, cancellationToken);
    return Results.Ok(response);
}).RequireAuthorization();

app.MapGet("/Manage/MyAccount", async (
    ClaimsPrincipal user,
    IIdentityAccountService accountService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var profile = await accountService.GetProfileAsync(userId.Value, cancellationToken);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
}).RequireAuthorization();

app.MapPost("/Manage/MyAccount", async (
    ClaimsPrincipal user,
    [FromBody] UpdateProfileRequest request,
    IIdentityAccountService accountService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var profile = await accountService.UpdateProfileAsync(userId.Value, request, cancellationToken);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
}).RequireAuthorization();

app.MapGet("/Manage/ChangePassword", () => Results.Ok(new AccountStatusResponse("Submit current and new password to POST /Manage/ChangePassword."))).RequireAuthorization();

app.MapPost("/Manage/ChangePassword", async (
    ClaimsPrincipal user,
    [FromBody] ChangePasswordRequest request,
    IIdentityAccountService accountService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var changed = await accountService.ChangePasswordAsync(userId.Value, request, cancellationToken);
    return changed ? Results.Ok(new AccountStatusResponse("Password changed.")) : Results.BadRequest(new AccountStatusResponse("Password change failed."));
}).RequireAuthorization();

app.MapGet("/Manage/SetPassword", () => Results.Ok(new AccountStatusResponse("Submit POST /Manage/SetPassword to set an initial password."))).RequireAuthorization();
app.MapPost("/Manage/SetPassword", async (
    ClaimsPrincipal user,
    [FromBody] ChangePasswordRequest request,
    IIdentityAccountService accountService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var changed = await accountService.ChangePasswordAsync(userId.Value, request, cancellationToken);
    return changed ? Results.Ok(new AccountStatusResponse("Password set.")) : Results.BadRequest(new AccountStatusResponse("Password set failed."));
}).RequireAuthorization();

app.MapPost("/Manage/SendVerificationEmail", () => Results.Ok(new AccountStatusResponse("Verification email queued."))).RequireAuthorization();
app.MapGet("/Manage/ExternalLogins", () => Results.Ok(new[] { "Google", "Microsoft" })).RequireAuthorization();
app.MapPost("/Manage/LinkLogin", () => Results.Ok(new AccountStatusResponse("External login link initiated."))).RequireAuthorization();
app.MapGet("/Manage/LinkLoginCallback", () => Results.Ok(new AccountStatusResponse("External login callback processed."))).RequireAuthorization();
app.MapPost("/Manage/RemoveLogin", () => Results.Ok(new AccountStatusResponse("External login removed."))).RequireAuthorization();
app.MapGet("/Manage/TwoFactorAuthentication", () => Results.Ok(new { enabled = false, recoveryCodesLeft = 0 })).RequireAuthorization();
app.MapGet("/Manage/Disable2faWarning", () => Results.Ok(new AccountStatusResponse("Two-factor disable warning."))).RequireAuthorization();
app.MapPost("/Manage/Disable2fa", () => Results.Ok(new AccountStatusResponse("Two-factor disabled."))).RequireAuthorization();
app.MapGet("/Manage/EnableAuthenticator", () => Results.Ok(new { sharedKey = "ESHOP-SHARED-KEY", authenticatorUri = "otpauth://totp/eshop" })).RequireAuthorization();
app.MapPost("/Manage/EnableAuthenticator", () => Results.Ok(new AccountStatusResponse("Authenticator enabled."))).RequireAuthorization();
app.MapGet("/Manage/ShowRecoveryCodes", () => Results.Ok(new RecoveryCodesResponse(["alpha-bravo", "charlie-delta", "echo-foxtrot"]))).RequireAuthorization();
app.MapGet("/Manage/ResetAuthenticatorWarning", () => Results.Ok(new AccountStatusResponse("Reset authenticator warning."))).RequireAuthorization();
app.MapPost("/Manage/ResetAuthenticator", () => Results.Ok(new AccountStatusResponse("Authenticator reset."))).RequireAuthorization();
app.MapPost("/Manage/GenerateRecoveryCodes", () => Results.Ok(new RecoveryCodesResponse(["golf-hotel", "india-juliet", "kilo-lima"]))).RequireAuthorization();
app.MapGet("/Manage/GenerateRecoveryCodesWarning", () => Results.Ok(new AccountStatusResponse("Generating new recovery codes will invalidate old ones."))).RequireAuthorization();

app.MapGet("/api/basket", async (
    ClaimsPrincipal user,
    BasketService basketService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var basket = await basketService.GetAsync(userId.Value, cancellationToken);
    return Results.Ok(basket);
}).RequireAuthorization();

app.MapPost("/api/basket/items", async (
    ClaimsPrincipal user,
    [FromBody] AddBasketItemRequest request,
    BasketService basketService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var basket = await basketService.AddItemAsync(userId.Value, request, cancellationToken);
    return Results.Ok(basket);
}).RequireAuthorization();

app.MapPut("/api/basket/items/{catalogItemId:guid}", async (
    Guid catalogItemId,
    ClaimsPrincipal user,
    [FromBody] UpdateBasketItemRequest request,
    BasketService basketService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var basket = await basketService.UpdateItemAsync(userId.Value, request with { CatalogItemId = catalogItemId }, cancellationToken);
    return Results.Ok(basket);
}).RequireAuthorization();

app.MapDelete("/api/basket/items/{catalogItemId:guid}", async (
    Guid catalogItemId,
    ClaimsPrincipal user,
    BasketService basketService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var basket = await basketService.RemoveItemAsync(userId.Value, catalogItemId, cancellationToken);
    return Results.Ok(basket);
}).RequireAuthorization();

app.MapGet("/Basket/Checkout", async (
    ClaimsPrincipal user,
    BasketService basketService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var basket = await basketService.GetAsync(userId.Value, cancellationToken);
    return Results.Ok(basket);
}).RequireAuthorization();

app.MapPost("/Basket/Checkout", async (
    ClaimsPrincipal user,
    [FromBody] CheckoutRequest request,
    BasketService basketService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    var username = user.Identity?.Name;
    if (userId is null || string.IsNullOrWhiteSpace(username))
    {
        return Results.Unauthorized();
    }

    var result = await basketService.CheckoutAsync(userId.Value, username, request, cancellationToken);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/Basket/Success", () => Results.Ok(new AccountStatusResponse("Checkout completed successfully."))).RequireAuthorization();

app.MapGet("/Order/MyOrders", async (
    ClaimsPrincipal user,
    OrderService orderService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var orders = await orderService.GetMyOrdersAsync(userId.Value, cancellationToken);
    return Results.Ok(orders);
}).RequireAuthorization();

// Loyalty endpoints
app.MapLoyaltyEndpoints();

app.MapGet("/Order/Detail/{orderId:guid}", async (
    Guid orderId,
    ClaimsPrincipal user,
    OrderService orderService,
    CancellationToken cancellationToken) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var order = await orderService.GetOrderDetailAsync(userId.Value, orderId, cancellationToken);
    return order is null ? Results.NotFound() : Results.Ok(order);
}).RequireAuthorization();

app.MapGet("/Admin", async (CatalogService catalogService, CancellationToken cancellationToken) =>
{
    var catalog = await catalogService.GetItemsAsync(new CatalogItemsQuery(0, 50), cancellationToken);
    return Results.Ok(new
    {
        title = "Admin Dashboard",
        catalog.totalCount,
        catalog.items
    });
}).RequireAuthorization("AdminOnly");

app.MapGet("/Admin/EditCatalogItem", async (
    [FromQuery] Guid catalogItemId,
    CatalogService catalogService,
    CancellationToken cancellationToken) =>
{
    var item = await catalogService.GetItemAsync(catalogItemId, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(item);
}).RequireAuthorization("AdminOnly");

app.MapPost("/api/payments", async (
    [FromBody] PaymentRequest request,
    IPaymentService paymentService,
    CancellationToken cancellationToken) =>
{
    var result = await paymentService.ProcessAsync(request, cancellationToken);
    return Results.Ok(result);
}).RequireAuthorization();

app.Run();

static Guid? GetUserId(ClaimsPrincipal user)
{
    var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
    return Guid.TryParse(raw, out var userId) ? userId : null;
}
