namespace TheGame.Core.Players;

public sealed class PlayerConcurrencyException(string playerId, Exception innerException)
    : Exception($"Player '{playerId}' was changed by another operation.", innerException);
