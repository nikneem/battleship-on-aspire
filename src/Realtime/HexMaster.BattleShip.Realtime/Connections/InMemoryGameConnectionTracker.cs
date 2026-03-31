using System.Collections.Concurrent;
using HexMaster.BattleShip.Realtime.Abstractions.Connections;

namespace HexMaster.BattleShip.Realtime.Connections;

public sealed class InMemoryGameConnectionTracker : IGameConnectionTracker
{
    private readonly ConcurrentDictionary<string, (string GameCode, string PlayerId)> _connections = new();

    public void TrackConnection(string connectionId, string gameCode, string playerId)
        => _connections[connectionId] = (gameCode, playerId);

    public bool TryGetConnection(string connectionId, out (string GameCode, string PlayerId) info)
        => _connections.TryGetValue(connectionId, out info);

    public void RemoveConnection(string connectionId)
        => _connections.TryRemove(connectionId, out _);
}
