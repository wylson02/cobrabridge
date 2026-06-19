namespace CobraBridge.CustomersService.Domain;

/// <summary>
/// A customer profile — a net-new modern capability. The COBOL core never
/// had a notion of customer/KYC; there is no legacy record to migrate or
/// strangle here. Unlike <c>Account</c>, this model lives only in
/// CustomersService: nothing else in the system needs to share it.
/// </summary>
public sealed record Customer
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    /// <summary>ISO 3166-1 alpha-2 country code (e.g. "CH", "LU").</summary>
    public required string Country { get; init; }
    public required KycStatus KycStatus { get; init; }
    public required CustomerStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public enum KycStatus
{
    Unknown = 0,
    Pending,
    Verified,
    Rejected
}

public enum CustomerStatus
{
    Unknown = 0,
    Active,
    Closed
}
