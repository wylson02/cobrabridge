using CobraBridge.Domain;

namespace CobraBridge.AccountsService.Persistence;

/// <summary>
/// Storage shape for an account row in PostgreSQL. Deliberately separate
/// from <see cref="Account"/>: that type is the wire/domain contract
/// (shared with the Bridge), this one is EF Core's mapped entity. Keeping
/// them apart means the shared Domain project stays persistence-agnostic.
/// </summary>
public class AccountEntity
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required AccountType Type { get; set; }
    public required long BalanceCents { get; set; }
    public required AccountStatus Status { get; set; }
}
