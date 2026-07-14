using ReactiveUI;
using System.Windows;
using the_game.ViewModels;

namespace the_game.Views;

public partial class AuthenticationView : ReactiveUserControl<AuthenticationViewModel>
{
    public AuthenticationView() => InitializeComponent();
    private void OnPasswordChanged(object sender, RoutedEventArgs e) { if (ViewModel is not null) ViewModel.Password = PasswordInput.Password; }
    private void OnConfirmationChanged(object sender, RoutedEventArgs e) { if (ViewModel is not null) ViewModel.Confirmation = ConfirmationInput.Password; }
}
