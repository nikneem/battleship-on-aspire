namespace HexMaster.BattleShip.Profiles.Abstractions.Validation;

public enum AnonymousPlayerSessionRenewalFailureReason
{
    TooEarly,
    SessionNotFound,
    InvalidSession
}

public sealed class AnonymousPlayerSessionRenewalException(
    string message,
    AnonymousPlayerSessionRenewalFailureReason reason) : Exception(message)
{
    public AnonymousPlayerSessionRenewalFailureReason Reason { get; } = reason;
}
