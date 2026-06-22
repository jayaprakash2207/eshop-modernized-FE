namespace BuildingBlocks.Security;

public sealed record CurrentUser(Guid UserId, string Username, string Role);

public interface ICurrentUserAccessor
{
    CurrentUser? GetCurrentUser();
}
