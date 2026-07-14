using ReactiveUI;
using System.Windows;
using the_game.ViewModels;

namespace the_game.Views;

public partial class BattleView : ReactiveUserControl<BattleViewModel>
{
    public BattleView() => InitializeComponent();

    private void OnLoaded(object sender, RoutedEventArgs e) =>
        ViewModel?.LoadCommand.Execute().Subscribe();
}
