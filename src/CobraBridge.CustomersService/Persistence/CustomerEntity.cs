using CobraBridge.CustomersService.Domain;

namespace CobraBridge.CustomersService.Persistence;

/// <summary>
/// Storage shape for a customer row in PostgreSQL. Kept separate from
/// <see cref="Customer"/> for the same reason AccountsService separates
/// AccountEntity from Account: the domain model stays persistence-agnostic.
/// </summary>
public class CustomerEntity
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Country { get; set; }
    public required KycStatus KycStatus { get; set; }
    public required CustomerStatus Status { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}
