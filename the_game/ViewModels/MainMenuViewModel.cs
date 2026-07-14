using ReactiveUI;
using System.Reactive;
using the_game.Lifecycle;
using the_game.Navigation;
using TheGame.Core.Players;

namespace the_game.ViewModels;

public sealed class MainMenuViewModel : ReactiveObject, IRoutableViewModel
{
    public MainMenuViewModel(
        ShellViewModel hostScreen,
        INavigationService navigation,
        IUserSession session,
        IApplicationLifetime applicationLifetime)
    {
        HostScreen = hostScreen;
        PlayCommand = ReactiveCommand.CreateFromTask(() => session.IsAuthenticated
            ? navigation.NavigateToAsync<ProfileViewModel>()
            : navigation.NavigateToAsync<AuthenticationViewModel>());
        SettingsCommand = ReactiveCommand.CreateFromTask(
            () => navigation.NavigateToAsync<SettingsViewModel>());
        AboutCommand = ReactiveCommand.CreateFromTask(
            () => navigation.NavigateToAsync<AboutViewModel>());
        ExitCommand = ReactiveCommand.Create(applicationLifetime.Exit);
    }

    public string? UrlPathSegment => "menu";

    public IScreen HostScreen { get; }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; }

    public ReactiveCommand<Unit, Unit> SettingsCommand { get; }

    public ReactiveCommand<Unit, Unit> AboutCommand { get; }

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
}
