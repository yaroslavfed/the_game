using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using TheGame.Core.Inventory;
using TheGame.Core.Players;
using the_game.Navigation;
using the_game.Presentation;

namespace the_game.ViewModels;

public sealed class StoreViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly IUserSession _session;
    private readonly IPlayerRepository _players;
    private readonly IInventoryCatalog _catalog;
    private readonly IStoreService _store;
    private InventoryItem? _selectedItem;
    private PlayerProfile? _player;
    private string? _message;
    private ScreenState _state = ScreenState.Loading();

    public StoreViewModel(ShellViewModel hostScreen, IUserSession session, IPlayerRepository players, IInventoryCatalog catalog, IStoreService store, INavigationService navigation)
    {
        HostScreen = hostScreen; _session = session; _players = players; _catalog = catalog; _store = store;
        LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        var hasSelection = this.WhenAnyValue(x => x.SelectedItem).Select(item => item is not null);
        BuyCommand = ReactiveCommand.CreateFromTask(BuyAsync, hasSelection);
        EquipCommand = ReactiveCommand.CreateFromTask(EquipAsync, hasSelection);
        BackCommand = ReactiveCommand.CreateFromTask(navigation.GoBackAsync);
    }

    public string? UrlPathSegment => "store";
    public IScreen HostScreen { get; }
    public ObservableCollection<InventoryItem> Items { get; } = [];
    public InventoryItem? SelectedItem { get => _selectedItem; set => this.RaiseAndSetIfChanged(ref _selectedItem, value); }
    public PlayerProfile? Player { get => _player; private set => this.RaiseAndSetIfChanged(ref _player, value); }
    public string? Message { get => _message; private set => this.RaiseAndSetIfChanged(ref _message, value); }
    public ScreenState State { get => _state; private set => this.RaiseAndSetIfChanged(ref _state, value); }
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> BuyCommand { get; }
    public ReactiveCommand<Unit, Unit> EquipCommand { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    private async Task LoadAsync(CancellationToken token)
    {
        State = ScreenState.Loading();
        try
        {
            if (_session.PlayerId is null)
            {
                State = ScreenState.Error("Сессия пользователя завершена");
                return;
            }

            Player = await _players.GetAsync(_session.PlayerId, token);
            if (Player is null)
            {
                State = ScreenState.Error("Профиль игрока не найден");
                return;
            }

            Items.Clear();
            foreach (InventoryItem item in await _catalog.GetAllAsync(token)) Items.Add(item);
            State = Items.Count == 0
                ? ScreenState.Empty("В магазине пока нет предметов")
                : ScreenState.Content();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception)
        {
            State = ScreenState.Error("Не удалось загрузить магазин. Попробуйте ещё раз.");
        }
    }

    private async Task BuyAsync(CancellationToken token)
    {
        if (_session.PlayerId is null || SelectedItem is null) return;
        StoreResult result = await _store.PurchaseAsync(_session.PlayerId, SelectedItem.Id, token);
        Message = result.IsSuccess ? "Предмет куплен" : result.Error;
        await LoadAsync(token);
    }

    private async Task EquipAsync(CancellationToken token)
    {
        if (_session.PlayerId is null || SelectedItem is null) return;
        StoreResult result = await _store.EquipAsync(_session.PlayerId, SelectedItem.Id, token);
        Message = result.IsSuccess ? "Предмет экипирован" : result.Error;
        await LoadAsync(token);
    }
}
