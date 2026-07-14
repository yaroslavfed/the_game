using ReactiveUI;
using the_game.ViewModels;

namespace the_game.Views;

public partial class AboutView : ReactiveUserControl<AboutViewModel>
{
    public AboutView() => InitializeComponent();
}
