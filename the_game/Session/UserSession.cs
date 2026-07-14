using ReactiveUI;
using TheGame.Core.Players;

namespace the_game.Session;

public sealed class UserSession : ReactiveObject, IUserSession
{
    private string? _playerId;
    public string? PlayerId => _playerId;
    public bool IsAuthenticated => _playerId is not null;

    public void SignIn(string playerId)
    {
        this.RaiseAndSetIfChanged(ref _playerId, playerId, nameof(PlayerId));
        this.RaisePropertyChanged(nameof(IsAuthenticated));
    }

    public void SignOut()
    {
        this.RaiseAndSetIfChanged(ref _playerId, null, nameof(PlayerId));
        this.RaisePropertyChanged(nameof(IsAuthenticated));
    }
}
