using ReactiveUI;
using System.Reactive;
using TheGame.Core.Authentication;
using TheGame.Core.Players;
using the_game.Navigation;

namespace the_game.ViewModels;

public sealed class AuthenticationViewModel : ReactiveObject, IRoutableViewModel
{
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _confirmation = string.Empty;
    private string? _message;
    private bool _isRegistration;

    public AuthenticationViewModel(ShellViewModel hostScreen, IAuthenticationService authentication, IUserSession session, INavigationService navigation)
    {
        HostScreen = hostScreen;
        SubmitCommand = ReactiveCommand.CreateFromTask(async cancellationToken =>
        {
            if (IsRegistration && Password != Confirmation) { Message = "Пароли не совпадают"; return; }
            AuthenticationResult result = IsRegistration
                ? await authentication.RegisterAsync(Login, Password, cancellationToken)
                : await authentication.SignInAsync(Login, Password, cancellationToken);
            Message = result.Error;
            if (result.IsSuccess && result.PlayerId is not null)
            {
                session.SignIn(result.PlayerId);
                await navigation.NavigateToAsync<ProfileViewModel>(cancellationToken);
                Password = Confirmation = string.Empty;
            }
        });
        ToggleModeCommand = ReactiveCommand.Create(() => { IsRegistration = !IsRegistration; Message = null; this.RaisePropertyChanged(nameof(Title)); });
        BackCommand = ReactiveCommand.CreateFromTask(navigation.GoBackAsync);
    }

    public string? UrlPathSegment => "authentication";
    public IScreen HostScreen { get; }
    public string Login { get => _login; set => this.RaiseAndSetIfChanged(ref _login, value); }
    public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }
    public string Confirmation { get => _confirmation; set => this.RaiseAndSetIfChanged(ref _confirmation, value); }
    public string? Message { get => _message; private set => this.RaiseAndSetIfChanged(ref _message, value); }
    public bool IsRegistration { get => _isRegistration; set => this.RaiseAndSetIfChanged(ref _isRegistration, value); }
    public string Title => IsRegistration ? "РЕГИСТРАЦИЯ" : "ВХОД";
    public ReactiveCommand<Unit, Unit> SubmitCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleModeCommand { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; }
}
