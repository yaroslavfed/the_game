using ReactiveUI;
using ReactiveUI.Builder;
using System.Reactive.Threading.Tasks;
using TheGame.Core.Battle;
using TheGame.Core.Inventory;
using TheGame.Core.Players;
using the_game.Lifecycle;
using the_game.Navigation;
using the_game.Session;
using the_game.ViewModels;
using Xunit;

namespace the_game.Tests.Battle;

public sealed class BattleViewModelLifecycleTests
{
    static BattleViewModelLifecycleTests()
    {
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
    }

    [Fact]
    public async Task Deactivation_CancelsPendingEnemyResponse()
    {
        var delay = new ControlledDelay();
        BattleViewModel viewModel = CreateViewModel(delay);
        using IDisposable activation = viewModel.Activator.Activate();
        await viewModel.LoadCommand.Execute().ToTask();
        viewModel.SelectedEnemy = viewModel.Enemies[0];
        double initialHeroHealth = viewModel.Session!.Hero.Health;

        Task attack = viewModel.AttackCommand.Execute().ToTask();
        await delay.Started.Task.WaitAsync(TestContext.Current.CancellationToken);
        activation.Dispose();
        await attack.WaitAsync(TestContext.Current.CancellationToken);

        Assert.True(delay.WasCancelled);
        Assert.Equal(initialHeroHealth, viewModel.Session.Hero.Health);
    }

    private static BattleViewModel CreateViewModel(IAsyncDelay delay)
    {
        var session = new UserSession();
        session.SignIn("player");
        return new BattleViewModel(
            new ShellViewModel(),
            session,
            new PlayerRepository(),
            new InventoryCatalog(),
            new EnemyCatalog(),
            new BattleEngine(),
            delay,
            new BattleTimingOptions(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)),
            new NavigationService());
    }

    private sealed class ControlledDelay : IAsyncDelay
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool WasCancelled { get; private set; }

        public async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                WasCancelled = true;
                throw;
            }
        }
    }

    private sealed class PlayerRepository : IPlayerRepository
    {
        private PlayerProfile _player = new("player", "Tester", 1, 0, 0, "101", ["101"], "201", ["201"]);
        public Task<PlayerProfile?> GetAsync(string playerId, CancellationToken cancellationToken = default) =>
            Task.FromResult<PlayerProfile?>(_player);
        public Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
        {
            _player = profile;
            return Task.CompletedTask;
        }
    }

    private sealed class InventoryCatalog : IInventoryCatalog
    {
        public Task<InventoryItem?> GetAsync(string itemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<InventoryItem?>(new(itemId, itemId.StartsWith('1') ? InventoryItemKind.Weapon : InventoryItemKind.Armor, 10, 0, 0));
        public Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<InventoryItem>>([]);
    }

    private sealed class EnemyCatalog : IEnemyCatalog
    {
        private static readonly EnemyDefinition Enemy = new("0", "Enemy", 100, 20, 0, 10);
        public Task<EnemyDefinition?> GetAsync(string rank, CancellationToken cancellationToken = default) =>
            Task.FromResult<EnemyDefinition?>(Enemy);
        public Task<IReadOnlyList<EnemyDefinition>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EnemyDefinition>>([Enemy]);
    }

    private sealed class NavigationService : INavigationService
    {
        public Task NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
            where TViewModel : class, IRoutableViewModel => Task.CompletedTask;
        public Task GoBackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
