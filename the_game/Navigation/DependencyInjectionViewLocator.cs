using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace the_game.Navigation;

public sealed class DependencyInjectionViewLocator(IServiceProvider services) : IViewLocator
{
    public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract = null)
        where TViewModel : class =>
        services.GetService<IViewFor<TViewModel>>();

    public IViewFor? ResolveView(object? instance, string? contract = null)
    {
        if (instance is null)
        {
            return null;
        }

        Type viewType = typeof(IViewFor<>).MakeGenericType(instance.GetType());
        return services.GetService(viewType) as IViewFor;
    }
}
