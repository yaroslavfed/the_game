using ReactiveUI;
using the_game.ViewModels;

namespace the_game.Views;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (ViewModel?.IsRebinding != true) return;
        ViewModel.ApplyKey(e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key);
        e.Handled = true;
    }
}
