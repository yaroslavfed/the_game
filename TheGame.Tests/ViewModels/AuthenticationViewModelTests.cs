using ReactiveUI;
using ReactiveUI.Builder;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using TheGame.Core.Authentication;
using TheGame.Core.Players;
using the_game.Lifecycle;
using the_game.Navigation;
using the_game.Session;
using the_game.ViewModels;
using Xunit;

namespace the_game.Tests.ViewModels;

public sealed class AuthenticationViewModelTests
{
    static AuthenticationViewModelTests()
    {
        RxAppBuilder.CreateReactiveUIBuilder().WithCoreServices().BuildApp();
    }

    [Fact]
    public async Task SubmitCommand_ActivatesOnlyForValidCredentials()
    {
        AuthenticationViewModel viewModel = CreateViewModel();

        Assert.False(await viewModel.SubmitCommand.CanExecute.FirstAsync());
        Assert.NotNull(viewModel.ValidationMessage);

        viewModel.Login = "player";
        viewModel.Password = "secret-123";

        Assert.True(await viewModel.SubmitCommand.CanExecute.FirstAsync());
        Assert.Null(viewModel.ValidationMessage);

        viewModel.IsRegistration = true;

        Assert.False(await viewModel.SubmitCommand.CanExecute.FirstAsync());
        Assert.Equal("Пароли не совпадают", viewModel.ValidationMessage);

        viewModel.Confirmation = "secret-123";

        Assert.True(await viewModel.SubmitCommand.CanExecute.FirstAsync());
    }

    private static AuthenticationViewModel CreateViewModel() => new(
        new ShellViewModel(new ErrorService()),
        new AuthenticationService(),
        new UserSession(),
        new NavigationService());

    private sealed class AuthenticationService : IAuthenticationService
    {
        public Task<AuthenticationResult> SignInAsync(string login, string password, CancellationToken cancellationToken = default) =>
            Task.FromResult(AuthenticationResult.Success(login));
        public Task<AuthenticationResult> RegisterAsync(string login, string password, CancellationToken cancellationToken = default) =>
            Task.FromResult(AuthenticationResult.Success(login));
    }

    private sealed class NavigationService : INavigationService
    {
        public Task NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default) where TViewModel : class, IRoutableViewModel => Task.CompletedTask;
        public Task GoBackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class ErrorService : IApplicationErrorService
    {
        public string? Message => null;
        public void Clear() { }
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void OnNext(Exception value) { }
    }
}
