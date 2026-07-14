namespace TheGame.Core.Players;

public sealed record PlayerProfile(
    string Id,
    string Nickname,
    int Level,
    int Experience,
    int Money,
    string EquippedWeaponId,
    IReadOnlyList<string> OwnedWeaponIds,
    string EquippedArmorId,
    IReadOnlyList<string> OwnedArmorIds,
    int Revision = 0);
