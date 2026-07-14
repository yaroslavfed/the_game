using TheGame.Core.Players;

namespace TheGame.Infrastructure.Players;

public sealed class MigratingPlayerRepository(
    JsonPlayerRepository modern,
    LegacyPlayerRepository legacy) : IPlayerRepository
{
    private readonly SemaphoreSlim _migrationGate = new(1, 1);

    public async Task<PlayerProfile?> GetAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        PlayerProfile? profile = await modern.GetAsync(playerId, cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        await _migrationGate.WaitAsync(cancellationToken);
        try
        {
            profile = await modern.GetAsync(playerId, cancellationToken);
            if (profile is not null)
            {
                return profile;
            }

            profile = await legacy.GetAsync(playerId, cancellationToken);
            if (profile is not null)
            {
                await modern.SaveAsync(profile, cancellationToken);
            }
            return profile;
        }
        finally
        {
            _migrationGate.Release();
        }
    }

    public Task SaveAsync(PlayerProfile profile, CancellationToken cancellationToken = default) =>
        modern.SaveAsync(profile, cancellationToken);
}
