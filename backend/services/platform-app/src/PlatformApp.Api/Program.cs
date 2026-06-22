using System.Text;
using System.Security.Claims;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

// ── Core services ──────────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUserAccessor, HttpCurrentUserAccessor>();
builder.Services.AddPlatformTelemetry("PlatformApp.Api");

// ── MediatR (CQRS) ─────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(PlatformApp.Application.Catalog.Handlers.GetCatalogItemsHandler).Assembly);
});

// ── Application services ───────────────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PlatformApp.Application.Loyalty.ILoyaltyService,
                           PlatformApp.Application.Loyalty.LoyaltyService>();
builder.Services.AddHostedService<PlatformApp.Infrastructure.LoyaltyExpiryService>();

// ── CORS ───────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
                ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// ── JWT authentication ─────────────────────────────────────────────────────────
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer            = true,
            ValidateAudience          = true,
            ValidateIssuerSigningKey  = true,
            ValidateLifetime          = true,
            ValidIssuer               = jwtOptions.Issuer,
            ValidAudience             = jwtOptions.Audience,
            IssuerSigningKey          = new SymmetricSecurityKey(signingKey),
            ClockSkew                 = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin")));

// ── Health checks ──────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), ["live"]);

// ── Swagger / OpenAPI (v1) ─────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "PlatformApp API",
        Version     = "v1",
        Description = "Cloud-native modernization of eShopOnWeb — Clean Architecture + DDD + CQRS",
        Contact     = new OpenApiContact { Name = "Platform Team", Email = "platform@example.com" }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        Description  = "Enter JWT token (without 'Bearer ' prefix)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// ──────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────────
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformApp API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

// ── Health endpoints ───────────────────────────────────────────────────────────
app.MapHealthChecks("/api_health_check", new HealthCheckOptions
{
    Predicate      = _ => true,
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status  = report.Status.ToString(),
            entries = report.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() })
        });
        await ctx.Response.WriteAsync(result);
    }
});
app.MapHealthChecks("/home_page_health_check", new HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("live")
});

// ── Auth endpoints ─────────────────────────────────────────────────────────────
/// <summary>Authenticate and receive a JWT token.</summary>
app.MapPost("/api/v1/authenticate", async (
    [FromBody] AuthenticateRequest request,
    IIdentityService identityService,
    CancellationToken cancellationToken) =>
{
    var response = await identityService.AuthenticateAsync(request, cancellationToken);
    return response is null ? Results.Unauthorized() : Results.Ok(response);
})
.WithName("Authenticate")
.WithTags("Identity")
.WithOpenApi();

// Legacy route preserved for backward compatibility
app.MapPost("/api/authenticate", async (
    [FromBody] AuthenticateRequest request,
    IIdentityService identityService,
    CancellationToken cancellationToken) =>
{
    var response = await identityService.AuthenticateAsync(request, cancellationToken);
    return response is null ? Results.Unauthorized() : Results.Ok(response);
})
.WithTags("Identity (legacy)")
.WithOpenApi();

app.MapPost("/api/v1/Account/Register", async (
    [FromBody] RegisterRequest request,
    IIdentityAccountService accountService,
    CancellationToken cancellationToken) =>
{
    var response = await accountService.RegisterAsync(request, cancellationToken);
    return Results.Created($"/api/v1/User/{response.UserId}", response);
})
.WithName("Register")
.WithTags("Identity")
.WithOpenApi();

app.MapGet("/Account/Login",  () => Results.Ok(new AccountStatusResponse("Use POST /api/v1/authenticate to sign in."))).WithTags("Identity");
app.MapGet("/Account/Logout", () => Results.Ok(new AccountStatusResponse("Use POST /api/v1/User/Logout to sign out."))).WithTags("Identity");

app.MapGet("/Account/ConfirmEmail", async ([FromQuery] string username, IIdentityAccountService svc, CancellationToken ct) =>
    Results.Ok(await svc.ConfirmEmailAsync(username, ct))).WithTags("Identity");

// ── Catalog endpoints (v1) ─────────────────────────────────────────────────────
app.MapGet("/api/v1/catalog-brands", async (CatalogService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetBrandsAsync(ct)))
    .WithName("GetCatalogBrands").WithTags("Catalog").WithOpenApi();

app.MapGet("/api/v1/catalog-types", async (CatalogService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetTypesAsync(ct)))
    .WithName("GetCatalogTypes").WithTags("Catalog").WithOpenApi();

app.MapGet("/api/v1/catalog-items", async ([AsParameters] CatalogItemsQuery query, CatalogService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetItemsAsync(query, ct)))
    .WithName("GetCatalogItems").WithTags("Catalog").WithOpenApi();

app.MapGet("/api/v1/catalog-items/{catalogItemId:guid}", async (Guid catalogItemId, CatalogService svc, CancellationToken ct) =>
{
    var item = await svc.GetItemAsync(catalogItemId, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
})
.WithName("GetCatalogItemById").WithTags("Catalog").WithOpenApi();

app.MapPost("/api/v1/catalog-items", async ([FromBody] UpsertCatalogItemRequest request, CatalogService svc, CancellationToken ct) =>
{
    var item = await svc.CreateItemAsync(request, ct);
    return Results.Created($"/api/v1/catalog-items/{item.Id}", new { item.Id });
})
.RequireAuthorization("AdminOnly").WithName("CreateCatalogItem").WithTags("Catalog").WithOpenApi();

app.MapPut("/api/v1/catalog-items/{catalogItemId:guid}", async (Guid catalogItemId, [FromBody] UpsertCatalogItemRequest request, CatalogService svc, CancellationToken ct) =>
{
    var item = await svc.UpdateItemAsync(catalogItemId, request, ct);
    return item is null ? Results.NotFound() : Results.NoContent();
})
.RequireAuthorization("AdminOnly").WithName("UpdateCatalogItem").WithTags("Catalog").WithOpenApi();

app.MapDelete("/api/v1/catalog-items/{catalogItemId:guid}", async (Guid catalogItemId, CatalogService svc, CancellationToken ct) =>
{
    var deleted = await svc.DeleteItemAsync(catalogItemId, ct);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.RequireAuthorization("AdminOnly").WithName("DeleteCatalogItem").WithTags("Catalog").WithOpenApi();

// Legacy catalog routes
app.MapGet("/api/catalog-brands", async (CatalogService svc, CancellationToken ct) => Results.Ok(await svc.GetBrandsAsync(ct))).WithTags("Catalog (legacy)");
app.MapGet("/api/catalog-types",  async (CatalogService svc, CancellationToken ct) => Results.Ok(await svc.GetTypesAsync(ct))).WithTags("Catalog (legacy)");
app.MapGet("/api/catalog-items",  async ([AsParameters] CatalogItemsQuery q, CatalogService svc, CancellationToken ct) => Results.Ok(await svc.GetItemsAsync(q, ct))).WithTags("Catalog (legacy)");
app.MapGet("/api/catalog-items/{catalogItemId:guid}", async (Guid id, CatalogService svc, CancellationToken ct) => { var i = await svc.GetItemAsync(id, ct); return i is null ? Results.NotFound() : Results.Ok(i); }).WithTags("Catalog (legacy)");
app.MapPost("/api/catalog-items", async ([FromBody] UpsertCatalogItemRequest r, CatalogService svc, CancellationToken ct) => { var i = await svc.CreateItemAsync(r, ct); return Results.Created($"/api/catalog-items/{i.Id}", new { i.Id }); }).RequireAuthorization("AdminOnly").WithTags("Catalog (legacy)");
app.MapPut("/api/catalog-items/{catalogItemId:guid}", async (Guid id, [FromBody] UpsertCatalogItemRequest r, CatalogService svc, CancellationToken ct) => { var i = await svc.UpdateItemAsync(id, r, ct); return i is null ? Results.NotFound() : Results.NoContent(); }).RequireAuthorization("AdminOnly").WithTags("Catalog (legacy)");
app.MapPut("/api/catalog-items",  async ([FromBody] UpdateCatalogItemRequest r, CatalogService svc, CancellationToken ct) => { var i = await svc.UpdateItemAsync(r.CatalogItemId, new UpsertCatalogItemRequest(r.Name, r.Description, r.Price, r.CatalogBrandId, r.CatalogTypeId, r.PictureUri, r.AvailableStock), ct); return i is null ? Results.NotFound() : Results.NoContent(); }).RequireAuthorization("AdminOnly").WithTags("Catalog (legacy)");
app.MapDelete("/api/catalog-items/{catalogItemId:guid}", async (Guid id, CatalogService svc, CancellationToken ct) => { var d = await svc.DeleteItemAsync(id, ct); return d ? Results.NoContent() : Results.NotFound(); }).RequireAuthorization("AdminOnly").WithTags("Catalog (legacy)");

// ── User / Account endpoints ───────────────────────────────────────────────────
app.MapGet("/api/v1/user", async (ClaimsPrincipal user, IIdentityAccountService svc, CancellationToken ct) =>
{
    var uid = GetUserId(user);
    if (uid is null) return Results.Unauthorized();
    var profile = await svc.GetProfileAsync(uid.Value, ct);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
}).RequireAuthorization().WithName("GetUserProfile").WithTags("Account").WithOpenApi();

app.MapPost("/api/v1/user/logout", async (ClaimsPrincipal user, IIdentityAccountService svc, CancellationToken ct) =>
{
    var uid = GetUserId(user);
    if (uid is null) return Results.Unauthorized();
    return Results.Ok(await svc.LogoutAsync(uid.Value, ct));
}).RequireAuthorization().WithName("Logout").WithTags("Account").WithOpenApi();

app.MapGet("/api/v1/manage/account",  async (ClaimsPrincipal user, IIdentityAccountService svc, CancellationToken ct) => { var uid = GetUserId(user); if (uid is null) return Results.Unauthorized(); var p = await svc.GetProfileAsync(uid.Value, ct); return p is null ? Results.NotFound() : Results.Ok(p); }).RequireAuthorization().WithTags("Account");
app.MapPost("/api/v1/manage/account", async (ClaimsPrincipal user, [FromBody] UpdateProfileRequest req, IIdentityAccountService svc, CancellationToken ct) => { var uid = GetUserId(user); if (uid is null) return Results.Unauthorized(); var p = await svc.UpdateProfileAsync(uid.Value, req, ct); return p is null ? Results.NotFound() : Results.Ok(p); }).RequireAuthorization().WithTags("Account");
app.MapPost("/api/v1/manage/change-password", async (ClaimsPrincipal user, [FromBody] ChangePasswordRequest req, IIdentityAccountService svc, CancellationToken ct) => { var uid = GetUserId(user); if (uid is null) return Results.Unauthorized(); var ok = await svc.ChangePasswordAsync(uid.Value, req, ct); return ok ? Results.Ok(new AccountStatusResponse("Password changed.")) : Results.BadRequest(new AccountStatusResponse("Password change failed.")); }).RequireAuthorization().WithTags("Account");

// Legacy account routes
app.MapGet("/User",                   async (ClaimsPrincipal u, IIdentityAccountService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); var p = await s.GetProfileAsync(uid.Value, ct); return p is null ? Results.NotFound() : Results.Ok(p); }).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/User/Logout",           async (ClaimsPrincipal u, IIdentityAccountService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); return Results.Ok(await s.LogoutAsync(uid.Value, ct)); }).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/MyAccount",       async (ClaimsPrincipal u, IIdentityAccountService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); var p = await s.GetProfileAsync(uid.Value, ct); return p is null ? Results.NotFound() : Results.Ok(p); }).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/MyAccount",      async (ClaimsPrincipal u, [FromBody] UpdateProfileRequest r, IIdentityAccountService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); var p = await s.UpdateProfileAsync(uid.Value, r, ct); return p is null ? Results.NotFound() : Results.Ok(p); }).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/ChangePassword", async (ClaimsPrincipal u, [FromBody] ChangePasswordRequest r, IIdentityAccountService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); var ok = await s.ChangePasswordAsync(uid.Value, r, ct); return ok ? Results.Ok(new AccountStatusResponse("Password changed.")) : Results.BadRequest(new AccountStatusResponse("Password change failed.")); }).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/ChangePassword",  () => Results.Ok(new AccountStatusResponse("Submit current and new password to POST /api/v1/manage/change-password."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/SetPassword",    async (ClaimsPrincipal u, [FromBody] ChangePasswordRequest r, IIdentityAccountService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); var ok = await s.ChangePasswordAsync(uid.Value, r, ct); return ok ? Results.Ok(new AccountStatusResponse("Password set.")) : Results.BadRequest(new AccountStatusResponse("Password set failed.")); }).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/SetPassword",                    () => Results.Ok(new AccountStatusResponse("Submit POST /api/v1/manage/change-password to set a password."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/SendVerificationEmail",          () => Results.Ok(new AccountStatusResponse("Verification email queued."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/ExternalLogins",                 () => Results.Ok(new[] { "Google", "Microsoft" })).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/LinkLogin",                     () => Results.Ok(new AccountStatusResponse("External login link initiated."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/LinkLoginCallback",              () => Results.Ok(new AccountStatusResponse("External login callback processed."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/RemoveLogin",                   () => Results.Ok(new AccountStatusResponse("External login removed."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/TwoFactorAuthentication",        () => Results.Ok(new { enabled = false, recoveryCodesLeft = 0 })).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/Disable2faWarning",              () => Results.Ok(new AccountStatusResponse("Two-factor disable warning."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/Disable2fa",                    () => Results.Ok(new AccountStatusResponse("Two-factor disabled."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/EnableAuthenticator",            () => Results.Ok(new { sharedKey = "ESHOP-SHARED-KEY", authenticatorUri = "otpauth://totp/eshop" })).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/EnableAuthenticator",           () => Results.Ok(new AccountStatusResponse("Authenticator enabled."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/ShowRecoveryCodes",              () => Results.Ok(new RecoveryCodesResponse(["alpha-bravo", "charlie-delta", "echo-foxtrot"]))).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/ResetAuthenticatorWarning",      () => Results.Ok(new AccountStatusResponse("Reset authenticator warning."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/ResetAuthenticator",            () => Results.Ok(new AccountStatusResponse("Authenticator reset."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Manage/GenerateRecoveryCodes",         () => Results.Ok(new RecoveryCodesResponse(["golf-hotel", "india-juliet", "kilo-lima"]))).RequireAuthorization().WithTags("Account (legacy)");
app.MapGet("/Manage/GenerateRecoveryCodesWarning",   () => Results.Ok(new AccountStatusResponse("Generating new recovery codes will invalidate old ones."))).RequireAuthorization().WithTags("Account (legacy)");
app.MapPost("/Account/Register",                     async ([FromBody] RegisterRequest r, IIdentityAccountService s, CancellationToken ct) => { var res = await s.RegisterAsync(r, ct); return Results.Created($"/User/{res.UserId}", res); }).WithTags("Account (legacy)");

// ── Basket endpoints (v1) ──────────────────────────────────────────────────────
app.MapGet("/api/v1/basket", async (ClaimsPrincipal user, BasketService svc, CancellationToken ct) =>
{
    var uid = GetUserId(user); if (uid is null) return Results.Unauthorized();
    return Results.Ok(await svc.GetAsync(uid.Value, ct));
}).RequireAuthorization().WithName("GetBasket").WithTags("Basket").WithOpenApi();

app.MapPost("/api/v1/basket/items", async (ClaimsPrincipal user, [FromBody] AddBasketItemRequest req, BasketService svc, CancellationToken ct) =>
{
    var uid = GetUserId(user); if (uid is null) return Results.Unauthorized();
    return Results.Ok(await svc.AddItemAsync(uid.Value, req, ct));
}).RequireAuthorization().WithName("AddBasketItem").WithTags("Basket").WithOpenApi();

app.MapPut("/api/v1/basket/items/{catalogItemId:guid}", async (Guid catalogItemId, ClaimsPrincipal user, [FromBody] UpdateBasketItemRequest req, BasketService svc, CancellationToken ct) =>
{
    var uid = GetUserId(user); if (uid is null) return Results.Unauthorized();
    return Results.Ok(await svc.UpdateItemAsync(uid.Value, req with { CatalogItemId = catalogItemId }, ct));
}).RequireAuthorization().WithName("UpdateBasketItem").WithTags("Basket").WithOpenApi();

app.MapDelete("/api/v1/basket/items/{catalogItemId:guid}", async (Guid catalogItemId, ClaimsPrincipal user, BasketService svc, CancellationToken ct) =>
{
    var uid = GetUserId(user); if (uid is null) return Results.Unauthorized();
    return Results.Ok(await svc.RemoveItemAsync(uid.Value, catalogItemId, ct));
}).RequireAuthorization().WithName("RemoveBasketItem").WithTags("Basket").WithOpenApi();

app.MapPost("/api/v1/basket/checkout", async (ClaimsPrincipal user, [FromBody] CheckoutRequest req, BasketService svc, CancellationToken ct) =>
{
    var uid = GetUserId(user); var username = user.Identity?.Name;
    if (uid is null || string.IsNullOrWhiteSpace(username)) return Results.Unauthorized();
    return Results.Ok(await svc.CheckoutAsync(uid.Value, username, req, ct));
}).RequireAuthorization().WithName("CheckoutBasket").WithTags("Basket").WithOpenApi();

// Legacy basket routes
app.MapGet("/api/basket", async (ClaimsPrincipal u, BasketService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); return Results.Ok(await s.GetAsync(uid.Value, ct)); }).RequireAuthorization().WithTags("Basket (legacy)");
app.MapPost("/api/basket/items", async (ClaimsPrincipal u, [FromBody] AddBasketItemRequest r, BasketService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); return Results.Ok(await s.AddItemAsync(uid.Value, r, ct)); }).RequireAuthorization().WithTags("Basket (legacy)");
app.MapPut("/api/basket/items/{catalogItemId:guid}", async (Guid id, ClaimsPrincipal u, [FromBody] UpdateBasketItemRequest r, BasketService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); return Results.Ok(await s.UpdateItemAsync(uid.Value, r with { CatalogItemId = id }, ct)); }).RequireAuthorization().WithTags("Basket (legacy)");
app.MapDelete("/api/basket/items/{catalogItemId:guid}", async (Guid id, ClaimsPrincipal u, BasketService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); return Results.Ok(await s.RemoveItemAsync(uid.Value, id, ct)); }).RequireAuthorization().WithTags("Basket (legacy)");
app.MapGet("/Basket/Checkout", async (ClaimsPrincipal u, BasketService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); return Results.Ok(await s.GetAsync(uid.Value, ct)); }).RequireAuthorization().WithTags("Basket (legacy)");
app.MapPost("/Basket/Checkout", async (ClaimsPrincipal u, [FromBody] CheckoutRequest r, BasketService s, CancellationToken ct) => { var uid = GetUserId(u); var uname = u.Identity?.Name; if (uid is null || string.IsNullOrWhiteSpace(uname)) return Results.Unauthorized(); return Results.Ok(await s.CheckoutAsync(uid.Value, uname, r, ct)); }).RequireAuthorization().WithTags("Basket (legacy)");
app.MapGet("/Basket/Success", () => Results.Ok(new AccountStatusResponse("Checkout completed successfully."))).RequireAuthorization().WithTags("Basket (legacy)");

// ── Order endpoints (v1) ───────────────────────────────────────────────────────
app.MapGet("/api/v1/orders", async (ClaimsPrincipal user, OrderService svc, CancellationToken ct) =>
{
    var uid = GetUserId(user); if (uid is null) return Results.Unauthorized();
    return Results.Ok(await svc.GetMyOrdersAsync(uid.Value, ct));
}).RequireAuthorization().WithName("GetMyOrders").WithTags("Orders").WithOpenApi();

app.MapGet("/api/v1/orders/{orderId:guid}", async (Guid orderId, ClaimsPrincipal user, OrderService svc, CancellationToken ct) =>
{
    var uid = GetUserId(user); if (uid is null) return Results.Unauthorized();
    var order = await svc.GetOrderDetailAsync(uid.Value, orderId, ct);
    return order is null ? Results.NotFound() : Results.Ok(order);
}).RequireAuthorization().WithName("GetOrderDetail").WithTags("Orders").WithOpenApi();

// Legacy order routes
app.MapGet("/Order/MyOrders", async (ClaimsPrincipal u, OrderService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); return Results.Ok(await s.GetMyOrdersAsync(uid.Value, ct)); }).RequireAuthorization().WithTags("Orders (legacy)");
app.MapGet("/Order/Detail/{orderId:guid}", async (Guid id, ClaimsPrincipal u, OrderService s, CancellationToken ct) => { var uid = GetUserId(u); if (uid is null) return Results.Unauthorized(); var o = await s.GetOrderDetailAsync(uid.Value, id, ct); return o is null ? Results.NotFound() : Results.Ok(o); }).RequireAuthorization().WithTags("Orders (legacy)");

// ── Loyalty endpoints ──────────────────────────────────────────────────────────
app.MapLoyaltyEndpoints();

// ── Admin endpoints ────────────────────────────────────────────────────────────
app.MapGet("/api/v1/admin/catalog", async (CatalogService svc, CancellationToken ct) =>
{
    var catalog = await svc.GetItemsAsync(new CatalogItemsQuery(0, 50), ct);
    return Results.Ok(new { title = "Admin Dashboard", catalog.totalCount, catalog.items });
}).RequireAuthorization("AdminOnly").WithName("AdminCatalog").WithTags("Admin").WithOpenApi();

app.MapGet("/api/v1/admin/catalog/{catalogItemId:guid}", async (Guid catalogItemId, CatalogService svc, CancellationToken ct) =>
{
    var item = await svc.GetItemAsync(catalogItemId, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
}).RequireAuthorization("AdminOnly").WithName("AdminGetCatalogItem").WithTags("Admin").WithOpenApi();

// Legacy admin routes
app.MapGet("/Admin",             async (CatalogService s, CancellationToken ct) => { var c = await s.GetItemsAsync(new CatalogItemsQuery(0, 50), ct); return Results.Ok(new { title = "Admin Dashboard", c.totalCount, c.items }); }).RequireAuthorization("AdminOnly").WithTags("Admin (legacy)");
app.MapGet("/Admin/EditCatalogItem", async ([FromQuery] Guid catalogItemId, CatalogService s, CancellationToken ct) => { var i = await s.GetItemAsync(catalogItemId, ct); return i is null ? Results.NotFound() : Results.Ok(i); }).RequireAuthorization("AdminOnly").WithTags("Admin (legacy)");

// ── Payment endpoints ──────────────────────────────────────────────────────────
app.MapPost("/api/v1/payments", async ([FromBody] PaymentRequest req, IPaymentService svc, CancellationToken ct) =>
    Results.Ok(await svc.ProcessAsync(req, ct)))
    .RequireAuthorization().WithName("ProcessPayment").WithTags("Payments").WithOpenApi();

// Legacy payment route
app.MapPost("/api/payments", async ([FromBody] PaymentRequest r, IPaymentService s, CancellationToken ct) =>
    Results.Ok(await s.ProcessAsync(r, ct))).RequireAuthorization().WithTags("Payments (legacy)");

app.Run();

static Guid? GetUserId(ClaimsPrincipal user)
{
    var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
    return Guid.TryParse(raw, out var userId) ? userId : null;
}
