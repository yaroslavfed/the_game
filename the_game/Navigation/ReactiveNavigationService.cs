using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using the_game.ViewModels;

namespace the_game.Navigation;

public sealed class ReactiveNavigationService(
    ShellViewModel shell,
    IServiceProvider services) : INavigationService
{
    public async Task NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
        where TViewModel : class, IRoutableViewModel
    {
        TViewModel viewModel = services.GetRequiredService<TViewModel>();

        await shell.Router.Navigate
            .Execute(viewModel)
            .FirstAsync()
            .ToTask(cancellationToken);
    }

    public async Task GoBackAsync(CancellationToken cancellationToken = default)
    {
        await shell.Router.NavigateBack
            .Execute()
            .FirstAsync()
            .ToTask(cancellationToken);
    }
}
