using ReactiveUI;
using System.Windows;
using the_game.ViewModels;

namespace the_game.Views;

public partial class StoreView : ReactiveUserControl<StoreViewModel>
{
    public StoreView() => InitializeComponent();
    private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel?.LoadCommand.Execute().Subscribe();
}
