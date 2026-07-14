namespace TheGame.Core.Players;

public interface IUserSession
{
    string? PlayerId { get; }
    bool IsAuthenticated { get; }
    void SignIn(string playerId);
    void SignOut();
}
