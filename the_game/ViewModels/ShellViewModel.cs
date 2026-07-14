using ReactiveUI;

namespace the_game.ViewModels;

public sealed class ShellViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new();
}
