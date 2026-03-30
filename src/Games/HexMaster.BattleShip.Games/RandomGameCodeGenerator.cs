using System.Security.Cryptography;
using HexMaster.BattleShip.Games.Abstractions.Services;

namespace HexMaster.BattleShip.Games;

public sealed class RandomGameCodeGenerator : IGameCodeGenerator
{
    public string GenerateCode()
    {
        return RandomNumberGenerator.GetInt32(10_000_000, 100_000_000).ToString();
    }
}
