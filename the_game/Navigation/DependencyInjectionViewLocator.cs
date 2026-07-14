using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Windows;

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
        IViewFor? view = services.GetService(viewType) as IViewFor;

        if (view is null)
        {
            return null;
        }

        view.ViewModel = instance;

        if (view is FrameworkElement element)
        {
            element.DataContext = instance;
        }

        return view;
    }
}
