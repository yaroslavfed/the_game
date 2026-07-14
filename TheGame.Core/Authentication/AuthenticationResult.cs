namespace TheGame.Core.Authentication;

public sealed record AuthenticationResult(bool IsSuccess, string? PlayerId, string? Error)
{
    public static AuthenticationResult Success(string playerId) => new(true, playerId, null);
    public static AuthenticationResult Failure(string error) => new(false, null, error);
}
