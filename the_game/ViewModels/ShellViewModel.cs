using ReactiveUI;
using System.Reactive;
using the_game.Lifecycle;

namespace the_game.ViewModels;

public sealed class ShellViewModel : ReactiveObject, IScreen
{
    public ShellViewModel(IApplicationErrorService errors)
    {
        Errors = errors;
        DismissErrorCommand = ReactiveCommand.Create(errors.Clear);
    }

    public RoutingState Router { get; } = new();

    public IApplicationErrorService Errors { get; }

    public ReactiveCommand<Unit, Unit> DismissErrorCommand { get; }
}
