using ReactiveUI;
using ReactiveUI.Builder;
using System.Reactive.Threading.Tasks;
using TheGame.Core.Players;
using the_game.Lifecycle;
using the_game.Navigation;
using the_game.Presentation;
using the_game.Session;
using the_game.ViewModels;
using Xunit;

namespace the_game.Tests.ViewModels;

public sealed class ProfileViewModelTests
{
    static ProfileViewModelTests()
    {
        RxAppBuilder.CreateReactiveUIBuilder().WithCoreServices().BuildApp();
    }

    [Fact]
    public async Task LoadCommand_ConvertsRepositoryFailureToLocalErrorState()
    {
        var session = new UserSession();
        session.SignIn("player");
        var viewModel = new ProfileViewModel(
            new ShellViewModel(new ErrorService()), session, new FailingRepository(), new NavigationService());

        await viewModel.LoadCommand.Execute().ToTask();

        Assert.Equal(ScreenStatus.Error, viewModel.State.Status);
        Assert.Equal("Не удалось загрузить профиль. Попробуйте ещё раз.", viewModel.State.Message);
    }

    private sealed class FailingRepository : IPlayerRepository
    {
        public Task<PlayerProfile?> GetAsync(string playerId, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Database unavailable");
        public Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default) => Task.CompletedTask;
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
