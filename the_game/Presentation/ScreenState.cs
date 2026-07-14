namespace the_game.Presentation;

public enum ScreenStatus
{
    Loading,
    Content,
    Empty,
    Error
}

public sealed record ScreenState(ScreenStatus Status, string? Message = null)
{
    public static ScreenState Loading(string message = "Загрузка…") => new(ScreenStatus.Loading, message);
    public static ScreenState Content() => new(ScreenStatus.Content);
    public static ScreenState Empty(string message) => new(ScreenStatus.Empty, message);
    public static ScreenState Error(string message) => new(ScreenStatus.Error, message);
}
