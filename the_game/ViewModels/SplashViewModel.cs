using ReactiveUI;
using System.Reactive;
using the_game.Navigation;

namespace the_game.ViewModels;

public sealed class SplashViewModel : ReactiveObject, IRoutableViewModel
{
    public SplashViewModel(ShellViewModel hostScreen, ILegacyNavigationBridge legacyNavigation)
    {
        HostScreen = hostScreen;
        ContinueCommand = ReactiveCommand.Create(legacyNavigation.OpenMainMenu);
    }

    public string? UrlPathSegment => "splash";

    public IScreen HostScreen { get; }

    public ReactiveCommand<Unit, Unit> ContinueCommand { get; }
}
