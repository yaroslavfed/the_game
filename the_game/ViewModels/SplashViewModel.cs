using ReactiveUI;
using System.Reactive;
using the_game.Navigation;

namespace the_game.ViewModels;

public sealed class SplashViewModel : ReactiveObject, IRoutableViewModel
{
    public SplashViewModel(ShellViewModel hostScreen, INavigationService navigation)
    {
        HostScreen = hostScreen;
        ContinueCommand = ReactiveCommand.CreateFromTask(
            () => navigation.NavigateToAsync<MainMenuViewModel>());
    }

    public string? UrlPathSegment => "splash";

    public IScreen HostScreen { get; }

    public ReactiveCommand<Unit, Unit> ContinueCommand { get; }
}
