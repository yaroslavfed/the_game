using ReactiveUI;
using System.Reactive;
using TheGame.Core.Players;
using the_game.Navigation;
using the_game.Presentation;

namespace the_game.ViewModels;

public sealed class ProfileViewModel : ReactiveObject, IRoutableViewModel
{
    private PlayerProfile? _profile;
    private ScreenState _state = ScreenState.Loading();

    public ProfileViewModel(ShellViewModel hostScreen, IUserSession session, IPlayerRepository players, INavigationService navigation)
    {
        HostScreen = hostScreen;
        LoadCommand = ReactiveCommand.CreateFromTask(async token =>
        {
            State = ScreenState.Loading();
            try
            {
                if (session.PlayerId is null)
                {
                    State = ScreenState.Error("Сессия пользователя завершена");
                    return;
                }

                Profile = await players.GetAsync(session.PlayerId, token);
                State = Profile is null
                    ? ScreenState.Empty("Профиль не найден")
                    : ScreenState.Content();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception)
            {
                State = ScreenState.Error("Не удалось загрузить профиль. Попробуйте ещё раз.");
            }
        });
        LogoutCommand = ReactiveCommand.CreateFromTask(async token =>
        {
            session.SignOut();
            await navigation.NavigateToAsync<MainMenuViewModel>(token);
        });
        StoreCommand = ReactiveCommand.CreateFromTask(() => navigation.NavigateToAsync<StoreViewModel>());
        PlayCommand = ReactiveCommand.CreateFromTask(() => navigation.NavigateToAsync<ModeSelectionViewModel>());
        KnowledgeCommand = ReactiveCommand.CreateFromTask(() => navigation.NavigateToAsync<KnowledgeBaseViewModel>());
    }

    public string? UrlPathSegment => "profile";
    public IScreen HostScreen { get; }
    public PlayerProfile? Profile { get => _profile; private set => this.RaiseAndSetIfChanged(ref _profile, value); }
    public ScreenState State { get => _state; private set => this.RaiseAndSetIfChanged(ref _state, value); }
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
    public ReactiveCommand<Unit, Unit> StoreCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<Unit, Unit> KnowledgeCommand { get; }
}
