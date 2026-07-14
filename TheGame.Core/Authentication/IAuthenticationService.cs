namespace TheGame.Core.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> SignInAsync(string login, string password, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> RegisterAsync(string login, string password, CancellationToken cancellationToken = default);
}
