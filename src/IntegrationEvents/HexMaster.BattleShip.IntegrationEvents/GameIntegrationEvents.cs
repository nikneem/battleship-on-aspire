namespace HexMaster.BattleShip.IntegrationEvents;

/// <summary>Outcome of a fired shot — duplicated here to keep this project dependency-free.</summary>
public enum ShotOutcome
{
    Miss = 0,
    Hit = 1,
    Sunk = 2
}

public sealed record GameCreatedIntegrationEvent(
    string GameCode,
    string HostPlayerId,
    string HostPlayerName) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record PlayerJoinedGameIntegrationEvent(
    string GameCode,
    string GuestPlayerId,
    string GuestPlayerName) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record PlayerMarkedReadyIntegrationEvent(
    string GameCode,
    string PlayerId) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

/// <summary>Fleet was submitted. MUST NOT carry ship coordinates.</summary>
public sealed record FleetSubmittedIntegrationEvent(
    string GameCode,
    string PlayerId) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record FleetLockedIntegrationEvent(
    string GameCode,
    string PlayerId) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record GameStartedIntegrationEvent(
    string GameCode,
    string FirstTurnPlayerId) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record ShotFiredIntegrationEvent(
    string GameCode,
    string FiringPlayerId,
    int TargetRow,
    int TargetColumn,
    ShotOutcome Outcome) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record GameFinishedIntegrationEvent(
    string GameCode,
    string WinnerPlayerId) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record GameCancelledIntegrationEvent(
    string GameCode,
    string CancelledByPlayerId) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}

public sealed record GameAbandonedIntegrationEvent(
    string GameCode,
    string AbandoningPlayerId) : IntegrationEvent
{
    public override string SchemaVersion => "1.0";
}
