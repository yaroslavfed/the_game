using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using TheGame.Core.Battle;
using TheGame.Core.Inventory;
using TheGame.Core.Players;
using the_game.Lifecycle;
using the_game.Navigation;
using System.Windows.Input;
using the_game.Input;

namespace the_game.ViewModels;

public sealed class BattleViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    private readonly IUserSession _userSession;
    private readonly IPlayerRepository _players;
    private readonly IInventoryCatalog _inventory;
    private readonly IEnemyCatalog _enemies;
    private readonly IBattleEngine _engine;
    private readonly IAsyncDelay _delay;
    private readonly BattleTimingOptions _timing;
    private IReadOnlyList<EnemyDefinition> _enemyDefinitions = [];
    private PlayerProfile? _player;
    private BattleSession? _session;
    private BattleEnemy? _selectedEnemy;
    private string? _message;
    private CancellationToken _lifecycleToken;
    private bool _isActive;
    private bool _isTurnInProgress;
    private bool _isHealCoolingDown;
    private int _wave = 1;

    public BattleViewModel(
        ShellViewModel hostScreen,
        IUserSession userSession,
        IPlayerRepository players,
        IInventoryCatalog inventory,
        IEnemyCatalog enemies,
        IBattleEngine engine,
        IAsyncDelay delay,
        BattleTimingOptions timing,
        INavigationService navigation,
        IKeyBindingService? bindings = null)
    {
        HostScreen = hostScreen;
        _userSession = userSession;
        _players = players;
        _inventory = inventory;
        _enemies = enemies;
        _engine = engine;
        _delay = delay;
        _timing = timing;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        var canAttack = this.WhenAnyValue(
            viewModel => viewModel.SelectedEnemy,
            viewModel => viewModel.Session,
            viewModel => viewModel.IsTurnInProgress,
            (enemy, session, isBusy) =>
                enemy?.IsAlive == true && session?.Status == BattleStatus.InProgress && !isBusy);
        AttackCommand = ReactiveCommand.CreateFromTask(AttackAsync, canAttack);
        var canHeal = this.WhenAnyValue(
            viewModel => viewModel.Session,
            viewModel => viewModel.IsTurnInProgress,
            viewModel => viewModel.IsHealCoolingDown,
            (session, isBusy, isCoolingDown) =>
                session?.Status == BattleStatus.InProgress &&
                _player?.Level >= 50 &&
                session.Hero.Health < session.Hero.MaxHealth &&
                !isBusy &&
                !isCoolingDown);
        HealCommand = ReactiveCommand.CreateFromTask(HealAsync, canHeal);
        BackCommand = ReactiveCommand.CreateFromTask(navigation.GoBackAsync);
        AttackKey = bindings?.GetKey(GameAction.Attack) ?? Key.A;
        HealKey = bindings?.GetKey(GameAction.Heal) ?? Key.H;
        ExitBattleKey = bindings?.GetKey(GameAction.ExitBattle) ?? Key.Escape;

        this.WhenActivated(disposables =>
        {
            var cancellation = new CancellationTokenSource();
            _lifecycleToken = cancellation.Token;
            _isActive = true;
            IsTurnInProgress = false;
            IsHealCoolingDown = false;
            disposables.Add(LoadCommand.Execute().Subscribe());
            disposables.Add(Disposable.Create(() =>
            {
                _isActive = false;
                cancellation.Cancel();
                cancellation.Dispose();
            }));
        });
    }

    public string? UrlPathSegment => "battle";
    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; } = new();
    public ObservableCollection<BattleEnemy> Enemies { get; } = [];
    public BattleSession? Session
    {
        get => _session;
        private set => this.RaiseAndSetIfChanged(ref _session, value);
    }
    public BattleEnemy? SelectedEnemy
    {
        get => _selectedEnemy;
        set => this.RaiseAndSetIfChanged(ref _selectedEnemy, value);
    }
    public string? Message
    {
        get => _message;
        private set => this.RaiseAndSetIfChanged(ref _message, value);
    }
    public bool IsTurnInProgress
    {
        get => _isTurnInProgress;
        private set => this.RaiseAndSetIfChanged(ref _isTurnInProgress, value);
    }
    public bool IsHealCoolingDown
    {
        get => _isHealCoolingDown;
        private set => this.RaiseAndSetIfChanged(ref _isHealCoolingDown, value);
    }
    public int Wave
    {
        get => _wave;
        private set => this.RaiseAndSetIfChanged(ref _wave, value);
    }
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AttackCommand { get; }
    public ReactiveCommand<Unit, Unit> HealCommand { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public Key AttackKey { get; }
    public Key HealKey { get; }
    public Key ExitBattleKey { get; }

    private async Task LoadAsync(CancellationToken commandToken)
    {
        using CancellationTokenSource cancellation = LinkToLifecycle(commandToken);
        CancellationToken token = cancellation.Token;
        if (_userSession.PlayerId is null)
        {
            Message = "Активный профиль не найден";
            return;
        }

        PlayerProfile? player = await _players.GetAsync(_userSession.PlayerId, token);
        IReadOnlyList<EnemyDefinition> definitions = await _enemies.GetAllAsync(token);
        if (player is null || definitions.Count == 0)
        {
            Message = "Не удалось подготовить бой";
            return;
        }

        InventoryItem? weapon = await _inventory.GetAsync(player.EquippedWeaponId, token);
        InventoryItem? armor = await _inventory.GetAsync(player.EquippedArmorId, token);
        token.ThrowIfCancellationRequested();
        var hero = new BattleHero(
            100 + player.Level * 4,
            100 + player.Level * 4,
            weapon?.Power ?? 10,
            armor?.Power ?? 0);
        _player = player;
        _enemyDefinitions = definitions;
        Wave = 1;
        StartWave(hero);
        Message = "Выберите противника";
    }

    private async Task AttackAsync(CancellationToken commandToken)
    {
        if (Session is null || SelectedEnemy is null)
        {
            return;
        }

        using CancellationTokenSource cancellation = LinkToLifecycle(commandToken);
        CancellationToken token = cancellation.Token;
        IsTurnInProgress = true;
        BattleSession updated = _engine.Attack(Session, SelectedEnemy.Id);
        ApplySession(updated);
        try
        {
            if (updated.Status == BattleStatus.InProgress)
            {
                Message = "Противник готовит ответ";
                await _delay.DelayAsync(_timing.EnemyResponseDelay, token);
                BattleEnemy? enemy = updated.Enemies.FirstOrDefault(candidate => candidate.IsAlive);
                if (enemy is not null)
                {
                    updated = _engine.EnemyAttack(updated, enemy.Id);
                    token.ThrowIfCancellationRequested();
                    ApplySession(updated);
                }
            }

            if (updated.Status == BattleStatus.Victory && _player is not null)
            {
                PlayerProfile rewarded = ApplyWaveReward(_player, updated.Reward);
                await _players.SaveAsync(rewarded, token);
                _player = rewarded with { Revision = rewarded.Revision + 1 };
                token.ThrowIfCancellationRequested();
                Wave++;
                StartWave(updated.Hero);
                Message = $"Волна {Wave}. Награда за предыдущую: {updated.Reward}";
                return;
            }
            token.ThrowIfCancellationRequested();
            Message = updated.Status switch
            {
                BattleStatus.Victory => $"Победа! Награда: {updated.Reward}",
                BattleStatus.Defeat => "Поражение",
                _ => "Ход завершён"
            };
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        finally
        {
            if (_isActive)
            {
                IsTurnInProgress = false;
            }
        }
    }

    private async Task HealAsync(CancellationToken commandToken)
    {
        if (Session is null)
        {
            return;
        }

        ApplySession(_engine.Heal(Session, 0.5));
        Message = "Здоровье восстановлено";
        IsHealCoolingDown = true;
        using CancellationTokenSource cancellation = LinkToLifecycle(commandToken);
        try
        {
            await _delay.DelayAsync(_timing.HealCooldown, cancellation.Token);
            if (_isActive)
            {
                IsHealCoolingDown = false;
                Message = "Лечение снова доступно";
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
    }

    private CancellationTokenSource LinkToLifecycle(CancellationToken commandToken) =>
        CancellationTokenSource.CreateLinkedTokenSource(commandToken, _lifecycleToken);

    private void StartWave(BattleHero hero)
    {
        int enemyCount = Math.Min(9, Wave + 2);
        BattleEnemy[] enemies = Enumerable.Range(0, enemyCount)
            .Select(index => BattleEnemy.Create(
                $"wave-{Wave}-enemy-{index + 1}",
                _enemyDefinitions[index % _enemyDefinitions.Count]))
            .ToArray();
        ApplySession(BattleSession.Start(hero, enemies));
    }

    private PlayerProfile ApplyWaveReward(PlayerProfile player, int reward)
    {
        int level = player.Level;
        int experience = player.Experience + reward;
        while (experience >= 100)
        {
            experience -= 100;
            level++;
        }
        return player with
        {
            Money = player.Money + reward,
            Experience = experience,
            Level = level,
            HighestWave = Math.Max(player.HighestWave, Wave)
        };
    }

    private void ApplySession(BattleSession session)
    {
        string? selectedId = SelectedEnemy?.Id;
        Session = session;
        Enemies.Clear();
        foreach (BattleEnemy enemy in session.Enemies)
        {
            Enemies.Add(enemy);
        }
        SelectedEnemy = Enemies.FirstOrDefault(enemy => enemy.Id == selectedId && enemy.IsAlive);
    }
}
