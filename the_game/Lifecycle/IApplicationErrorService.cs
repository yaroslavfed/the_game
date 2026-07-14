namespace the_game.Lifecycle;

public interface IApplicationErrorService : IObserver<Exception>
{
    string? Message { get; }

    void Clear();
}
