using ReactiveUI;
using System.Reactive;
using the_game.Navigation;

namespace the_game.ViewModels;

public sealed class AboutViewModel : ReactiveObject, IRoutableViewModel
{
    public AboutViewModel(ShellViewModel hostScreen, INavigationService navigation)
    {
        HostScreen = hostScreen;
        BackCommand = ReactiveCommand.CreateFromTask(navigation.GoBackAsync);
    }

    public string? UrlPathSegment => "about";

    public IScreen HostScreen { get; }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }
}
