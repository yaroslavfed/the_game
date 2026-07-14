using ReactiveUI;
using System.Reactive;
using the_game.Navigation;
using System.Windows.Input;
using the_game.Input;

namespace the_game.ViewModels;

public sealed class SettingsViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly IKeyBindingService _bindings;
    private GameAction? _pendingAction;
    private string? _statusMessage;

    public SettingsViewModel(ShellViewModel hostScreen, INavigationService navigation, IKeyBindingService bindings)
    {
        HostScreen = hostScreen;
        _bindings = bindings;
        BackCommand = ReactiveCommand.CreateFromTask(navigation.GoBackAsync);
        BeginRebindCommand = ReactiveCommand.Create<GameAction>(BeginRebind);
        ResetCommand = ReactiveCommand.Create(Reset);
    }

    public string? UrlPathSegment => "settings";

    public IScreen HostScreen { get; }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<GameAction, Unit> BeginRebindCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }
    public Key AttackKey => _bindings.GetKey(GameAction.Attack);
    public Key HealKey => _bindings.GetKey(GameAction.Heal);
    public Key ExitBattleKey => _bindings.GetKey(GameAction.ExitBattle);
    public bool IsRebinding => _pendingAction is not null;
    public string? StatusMessage
    {
        get => _statusMessage;
        private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public void ApplyKey(Key key)
    {
        if (_pendingAction is not { } action || !KeyBindingService.IsAllowed(key)) return;
        _bindings.SetKey(action, key);
        _pendingAction = null;
        Refresh();
        StatusMessage = $"Клавиша назначена: {key}";
    }

    private void BeginRebind(GameAction action)
    {
        _pendingAction = action;
        this.RaisePropertyChanged(nameof(IsRebinding));
        StatusMessage = "Нажмите новую клавишу…";
    }

    private void Reset()
    {
        _bindings.Reset();
        _pendingAction = null;
        Refresh();
        StatusMessage = "Назначения восстановлены";
    }

    private void Refresh()
    {
        this.RaisePropertyChanged(nameof(AttackKey));
        this.RaisePropertyChanged(nameof(HealKey));
        this.RaisePropertyChanged(nameof(ExitBattleKey));
        this.RaisePropertyChanged(nameof(IsRebinding));
    }
}
