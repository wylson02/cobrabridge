namespace CobraBridge.Domain;

/// <summary>
/// The modern, clean representation of an account — what the rest of the
/// system speaks, whether it's served from the legacy COBOL master or from
/// PostgreSQL. The legacy fixed-width record never escapes the bridge.
/// </summary>
public sealed record Account
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required AccountType Type { get; init; }
    /// <summary>Balance in the account's minor unit (cents), to avoid float drift.</summary>
    public required long BalanceCents { get; init; }
    public required AccountStatus Status { get; init; }

    public decimal Balance => BalanceCents / 100m;
}

public enum AccountType
{
    Unknown = 0,
    Checking,
    Savings
}

public enum AccountStatus
{
    Unknown = 0,
    Active,
    Closed,
    Frozen
}
