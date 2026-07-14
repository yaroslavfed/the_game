namespace TheGame.Core.Storage;

public interface IContentDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
