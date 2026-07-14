using ReactiveUI;
using the_game.ViewModels;

namespace the_game.Views;

public partial class ShellWindow : ReactiveWindow<ShellViewModel>
{
    public ShellWindow(ShellViewModel viewModel, IViewLocator viewLocator)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = viewModel;
        ViewHost.Router = viewModel.Router;
        ViewHost.ViewLocator = viewLocator;
    }
}
