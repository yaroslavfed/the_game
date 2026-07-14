namespace TheGame.Core.Storage;

public interface IUserDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
