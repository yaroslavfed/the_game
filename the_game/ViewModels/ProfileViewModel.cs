using ReactiveUI;
using System.Reactive;
using TheGame.Core.Players;
using the_game.Navigation;

namespace the_game.ViewModels;

public sealed class ProfileViewModel : ReactiveObject, IRoutableViewModel
{
    private PlayerProfile? _profile;
    private string? _error;

    public ProfileViewModel(ShellViewModel hostScreen, IUserSession session, IPlayerRepository players, INavigationService navigation)
    {
        HostScreen = hostScreen;
        LoadCommand = ReactiveCommand.CreateFromTask(async token =>
        {
            if (session.PlayerId is null) return;
            Profile = await players.GetAsync(session.PlayerId, token);
            Error = Profile is null ? "Профиль не найден" : null;
        });
        LogoutCommand = ReactiveCommand.CreateFromTask(async token =>
        {
            session.SignOut();
            await navigation.NavigateToAsync<MainMenuViewModel>(token);
        });
        StoreCommand = ReactiveCommand.CreateFromTask(() => navigation.NavigateToAsync<StoreViewModel>());
        PlayCommand = ReactiveCommand.CreateFromTask(() => navigation.NavigateToAsync<ModeSelectionViewModel>());
        KnowledgeCommand = ReactiveCommand.CreateFromTask(() => navigation.NavigateToAsync<KnowledgeBaseViewModel>());
    }

    public string? UrlPathSegment => "profile";
    public IScreen HostScreen { get; }
    public PlayerProfile? Profile { get => _profile; private set => this.RaiseAndSetIfChanged(ref _profile, value); }
    public string? Error { get => _error; private set => this.RaiseAndSetIfChanged(ref _error, value); }
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
    public ReactiveCommand<Unit, Unit> StoreCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Unit, Unit> KnowledgeCommand { get; }
}
