using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace the_game.Lifecycle;

public sealed class ApplicationErrorService(ILogger<ApplicationErrorService> logger)
    : ReactiveObject, IApplicationErrorService
{
    private string? _message;

    public string? Message
    {
        get => _message;
        private set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public void OnNext(Exception value)
    {
        logger.LogError(value, "Unhandled application operation failed");
        Message = "Операция не выполнена. Попробуйте ещё раз.";
    }

    public void OnError(Exception error) => OnNext(error);

    public void OnCompleted() { }

    public void Clear() => Message = null;
}
