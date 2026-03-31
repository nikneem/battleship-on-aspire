using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games;

internal sealed class SystemRandomProvider : IRandomProvider
{
    public bool NextBool() => Random.Shared.Next(2) == 0;
}
