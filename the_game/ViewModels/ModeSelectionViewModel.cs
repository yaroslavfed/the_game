using ReactiveUI;
using System.Reactive;
using the_game.Navigation;

namespace the_game.ViewModels;

public sealed class ModeSelectionViewModel : ReactiveObject, IRoutableViewModel
{
    public ModeSelectionViewModel(ShellViewModel hostScreen, INavigationService navigation)
    {
        HostScreen = hostScreen;
        StartSinglePlayerCommand = ReactiveCommand.CreateFromTask(
            () => navigation.NavigateToAsync<BattleViewModel>());
        BackCommand = ReactiveCommand.CreateFromTask(navigation.GoBackAsync);
    }

    public string? UrlPathSegment => "modes";
    public IScreen HostScreen { get; }
    public ReactiveCommand<Unit, Unit> StartSinglePlayerCommand { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; }
}
