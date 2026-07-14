namespace the_game.Lifecycle;

public interface IAsyncDelay
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
}

public sealed class SystemAsyncDelay : IAsyncDelay
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken) =>
        Task.Delay(delay, TimeProvider.System, cancellationToken);
}
