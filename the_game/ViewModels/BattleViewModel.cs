using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using TheGame.Core.Battle;
using TheGame.Core.Inventory;
using TheGame.Core.Players;
using the_game.Navigation;

namespace the_game.ViewModels;

public sealed class BattleViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly IUserSession _userSession;
    private readonly IPlayerRepository _players;
    private readonly IInventoryCatalog _inventory;
    private readonly IEnemyCatalog _enemies;
    private readonly IBattleEngine _engine;
    private PlayerProfile? _player;
    private BattleSession? _session;
    private BattleEnemy? _selectedEnemy;
    private string? _message;

    public BattleViewModel(
        ShellViewModel hostScreen,
        IUserSession userSession,
        IPlayerRepository players,
        IInventoryCatalog inventory,
        IEnemyCatalog enemies,
        IBattleEngine engine,
        INavigationService navigation)
    {
        HostScreen = hostScreen;
        _userSession = userSession;
        _players = players;
        _inventory = inventory;
        _enemies = enemies;
        _engine = engine;

        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        var canAttack = this.WhenAnyValue(
            viewModel => viewModel.SelectedEnemy,
            viewModel => viewModel.Session,
            (enemy, session) => enemy?.IsAlive == true && session?.Status == BattleStatus.InProgress);
        AttackCommand = ReactiveCommand.CreateFromTask(AttackAsync, canAttack);
        HealCommand = ReactiveCommand.Create(Heal, this.WhenAnyValue(
            viewModel => viewModel.Session,
            session => session?.Status == BattleStatus.InProgress && session.Hero.Health < session.Hero.MaxHealth));
        BackCommand = ReactiveCommand.CreateFromTask(navigation.GoBackAsync);
    }

    public string? UrlPathSegment => "battle";
    public IScreen HostScreen { get; }
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
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> AttackCommand { get; }
    public ReactiveCommand<Unit, Unit> HealCommand { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (_userSession.PlayerId is null)
        {
            Message = "Активный профиль не найден";
            return;
        }

        PlayerProfile? player = await _players.GetAsync(_userSession.PlayerId, cancellationToken);
        IReadOnlyList<EnemyDefinition> definitions = await _enemies.GetAllAsync(cancellationToken);
        if (player is null || definitions.Count == 0)
        {
            Message = "Не удалось подготовить бой";
            return;
        }

        InventoryItem? weapon = await _inventory.GetAsync(player.EquippedWeaponId, cancellationToken);
        InventoryItem? armor = await _inventory.GetAsync(player.EquippedArmorId, cancellationToken);
        var hero = new BattleHero(
            100 + player.Level * 4,
            100 + player.Level * 4,
            weapon?.Power ?? 10,
            armor?.Power ?? 0);
        BattleEnemy[] wave = definitions
            .Take(3)
            .Select((definition, index) => BattleEnemy.Create($"enemy-{index + 1}", definition))
            .ToArray();

        ApplySession(BattleSession.Start(hero, wave));
        _player = player;
        Message = "Выберите противника";
    }

    private async Task AttackAsync(CancellationToken cancellationToken)
    {
        if (Session is null || SelectedEnemy is null)
        {
            return;
        }

        BattleSession updated = _engine.Attack(Session, SelectedEnemy.Id);
        if (updated.Status == BattleStatus.InProgress)
        {
            BattleEnemy? retaliatingEnemy = updated.Enemies.FirstOrDefault(enemy => enemy.IsAlive);
            if (retaliatingEnemy is not null)
            {
                updated = _engine.EnemyAttack(updated, retaliatingEnemy.Id);
            }
        }

        ApplySession(updated);
        if (updated.Status == BattleStatus.Victory && _player is not null)
        {
            _player = _player with { Money = _player.Money + updated.Reward };
            await _players.SaveAsync(_player, cancellationToken);
        }
        Message = updated.Status switch
        {
            BattleStatus.Victory => $"Победа! Награда: {updated.Reward}",
            BattleStatus.Defeat => "Поражение",
            _ => "Ход завершён"
        };
    }

    private void Heal()
    {
        if (Session is null)
        {
            return;
        }

        ApplySession(_engine.Heal(Session, 0.5));
        Message = "Здоровье восстановлено";
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
