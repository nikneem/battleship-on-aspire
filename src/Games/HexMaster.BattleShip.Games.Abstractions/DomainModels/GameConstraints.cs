namespace HexMaster.BattleShip.Games.Abstractions.DomainModels;

public static class GameConstraints
{
    public const int BoardSize = 10;
    public const int GameCodeLength = 8;
    public const int MaxPlayerNameLength = 40;

    public static IReadOnlyList<int> RequiredShipLengths { get; } = [5, 4, 3, 3, 2];
}
