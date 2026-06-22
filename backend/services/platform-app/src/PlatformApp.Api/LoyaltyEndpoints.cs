using Microsoft.AspNetCore.Mvc;
using PlatformApp.Application.Loyalty;

namespace PlatformApp.Api;

public static class LoyaltyEndpoints
{
    public static void MapLoyaltyEndpoints(this WebApplication app)
    {
        app.MapGet("/api/loyalty/balance", async (
            ClaimsPrincipal user,
            LoyaltyService loyaltyService,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(user);
            if (userId is null) return Results.Unauthorized();
            var balance = await loyaltyService.GetBalanceAsync(userId.Value, cancellationToken);
            return Results.Ok(balance);
        }).RequireAuthorization();

        app.MapGet("/api/loyalty/history", async (
            ClaimsPrincipal user,
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize,
            LoyaltyService loyaltyService,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(user);
            if (userId is null) return Results.Unauthorized();
            var history = await loyaltyService.GetHistoryAsync(userId.Value, pageIndex, pageSize, cancellationToken);
            return Results.Ok(history);
        }).RequireAuthorization();

        app.MapPost("/api/loyalty/redeem", async (
            ClaimsPrincipal user,
            [FromBody] RedeemPointsRequest request,
            LoyaltyService loyaltyService,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(user);
            if (userId is null) return Results.Unauthorized();
            await loyaltyService.RedeemPointsAsync(userId.Value, request, cancellationToken);
            return Results.Ok();
        }).RequireAuthorization();

        app.MapPost("/api/loyalty/earn", async (
            ClaimsPrincipal user,
            [FromBody] EarnPointsRequest request,
            LoyaltyService loyaltyService,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(user);
            if (userId is null) return Results.Unauthorized();
            var points = await loyaltyService.EarnPointsAsync(userId.Value, request, cancellationToken);
            return Results.Ok(new { points });
        }).RequireAuthorization();
    }
}
