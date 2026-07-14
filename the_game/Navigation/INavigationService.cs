using ReactiveUI;

namespace the_game.Navigation;

public interface INavigationService
{
    Task NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IRoutableViewModel;

    Task GoBackAsync(CancellationToken cancellationToken = default);
}
