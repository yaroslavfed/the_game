namespace the_game.Lifecycle;

public sealed class WpfApplicationLifetime : IApplicationLifetime
{
    public void Exit() => System.Windows.Application.Current.Shutdown();
}
