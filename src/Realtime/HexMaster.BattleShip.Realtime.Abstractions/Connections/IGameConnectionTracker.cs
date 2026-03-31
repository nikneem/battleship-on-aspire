namespace HexMaster.BattleShip.Realtime.Abstractions.Connections;

public interface IGameConnectionTracker
{
    void TrackConnection(string connectionId, string gameCode, string playerId);
    bool TryGetConnection(string connectionId, out (string GameCode, string PlayerId) info);
    void RemoveConnection(string connectionId);
}
