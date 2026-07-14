using ReactiveUI;
using the_game.ViewModels;

namespace the_game.Views;

public partial class MainMenuView : ReactiveUserControl<MainMenuViewModel>
{
    public MainMenuView() => InitializeComponent();
}
