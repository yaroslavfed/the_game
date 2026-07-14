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

    [Fact]
    public async Task CompletingWaves_IncreasesEnemyCountAndPersistsProgressRepeatedly()
    {
        var repository = new PlayerRepository();
        BattleViewModel viewModel = CreateViewModel(
            new ImmediateDelay(),
            repository,
            new InventoryCatalog(power: 200),
            new EnemyCatalog(new EnemyDefinition("0", "Enemy", 1, 0, 0, 60)));
        using IDisposable activation = viewModel.Activator.Activate();
        await viewModel.LoadCommand.Execute().ToTask();

        await CompleteCurrentWaveAsync(viewModel);

        Assert.Equal(2, viewModel.Wave);
        Assert.Equal(4, viewModel.Enemies.Count);
        Assert.Equal(180, repository.Player.Money);
        Assert.Equal(2, repository.Player.Level);
        Assert.Equal(80, repository.Player.Experience);
        Assert.Equal(1, repository.Player.HighestWave);

        await CompleteCurrentWaveAsync(viewModel);

        Assert.Equal(3, viewModel.Wave);
        Assert.Equal(5, viewModel.Enemies.Count);
        Assert.Equal(420, repository.Player.Money);
        Assert.Equal(5, repository.Player.Level);
        Assert.Equal(20, repository.Player.Experience);
        Assert.Equal(2, repository.Player.HighestWave);
        Assert.Equal(2, repository.SaveCount);
    }

    private static async Task CompleteCurrentWaveAsync(BattleViewModel viewModel)
    {
        foreach (BattleEnemy enemy in viewModel.Enemies.ToArray())
        {
            viewModel.SelectedEnemy = enemy;
            await viewModel.AttackCommand.Execute().ToTask();
        }
    }

    private static BattleViewModel CreateViewModel(
        IAsyncDelay delay,
        PlayerRepository? repository = null,
        InventoryCatalog? inventory = null,
        EnemyCatalog? enemies = null)
    {
        var session = new UserSession();
        session.SignIn("player");
        return new BattleViewModel(
            new ShellViewModel(new TestErrorService()),
            session,
            repository ?? new PlayerRepository(),
            inventory ?? new InventoryCatalog(),
            enemies ?? new EnemyCatalog(),
            new BattleEngine(),
            delay,
            new BattleTimingOptions(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)),
            new NavigationService());
    }

    private sealed class TestErrorService : IApplicationErrorService
    {
        public string? Message => null;
        public void Clear() { }
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void OnNext(Exception value) { }
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

    private sealed class ImmediateDelay : IAsyncDelay
    {
        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class PlayerRepository : IPlayerRepository
    {
        private PlayerProfile _player = new("player", "Tester", 1, 0, 0, "101", ["101"], "201", ["201"]);
        public PlayerProfile Player => _player;
        public int SaveCount { get; private set; }
        public Task<PlayerProfile?> GetAsync(string playerId, CancellationToken cancellationToken = default) =>
            Task.FromResult<PlayerProfile?>(_player);
        public Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
        {
            SaveCount++;
            _player = profile with { Revision = profile.Revision + 1 };
            return Task.CompletedTask;
        }
    }

    private sealed class InventoryCatalog(int power = 10) : IInventoryCatalog
    {
        public Task<InventoryItem?> GetAsync(string itemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<InventoryItem?>(new(itemId, itemId.StartsWith('1') ? InventoryItemKind.Weapon : InventoryItemKind.Armor, power, 0, 0));
        public Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<InventoryItem>>([]);
    }

    private sealed class EnemyCatalog(EnemyDefinition? enemy = null) : IEnemyCatalog
    {
        private readonly EnemyDefinition _enemy = enemy ?? new("0", "Enemy", 100, 20, 0, 10);
        public Task<EnemyDefinition?> GetAsync(string rank, CancellationToken cancellationToken = default) =>
            Task.FromResult<EnemyDefinition?>(_enemy);
        public Task<IReadOnlyList<EnemyDefinition>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EnemyDefinition>>([_enemy]);
    }

    private sealed class NavigationService : INavigationService
    {
        public Task NavigateToAsync<TViewModel>(CancellationToken cancellationToken = default)
            where TViewModel : class, IRoutableViewModel => Task.CompletedTask;
        public Task GoBackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
