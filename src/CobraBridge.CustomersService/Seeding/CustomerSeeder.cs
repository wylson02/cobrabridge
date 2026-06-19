using CobraBridge.CustomersService.Domain;
using CobraBridge.CustomersService.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CobraBridge.CustomersService.Seeding;

/// <summary>
/// Seeds a realistic starter set of customers. Unlike AccountsService's
/// LegacySeeder, this isn't migrating anything — Customers is a net-new
/// capability with no COBOL-side source of truth, so the seed data is just
/// a fixed, representative sample (CH/LU-private-banking flavored, several
/// countries, the full spread of KYC statuses).
///
/// Idempotent: only inserts customers whose Id isn't already present, so
/// re-running it on every container start is a safe no-op after the first.
/// </summary>
public static class CustomerSeeder
{
    private static readonly Customer[] SampleCustomers =
    [
        new() { Id = "CUST000001", Name = "Élodie Favre", Email = "elodie.favre@example.ch", Country = "CH", KycStatus = KycStatus.Verified, Status = CustomerStatus.Active, CreatedAt = new DateTimeOffset(2023, 02, 14, 9, 0, 0, TimeSpan.Zero) },
        new() { Id = "CUST000002", Name = "Jean-Marc Dupont", Email = "jm.dupont@example.lu", Country = "LU", KycStatus = KycStatus.Verified, Status = CustomerStatus.Active, CreatedAt = new DateTimeOffset(2023, 05, 03, 9, 0, 0, TimeSpan.Zero) },
        new() { Id = "CUST000003", Name = "Anna Mueller", Email = "anna.mueller@example.ch", Country = "CH", KycStatus = KycStatus.Verified, Status = CustomerStatus.Active, CreatedAt = new DateTimeOffset(2023, 07, 21, 9, 0, 0, TimeSpan.Zero) },
        new() { Id = "CUST000004", Name = "Lucas Bernard", Email = "lucas.bernard@example.fr", Country = "FR", KycStatus = KycStatus.Pending, Status = CustomerStatus.Active, CreatedAt = new DateTimeOffset(2024, 01, 09, 9, 0, 0, TimeSpan.Zero) },
        new() { Id = "CUST000005", Name = "Sophie Weber", Email = "sophie.weber@example.de", Country = "DE", KycStatus = KycStatus.Pending, Status = CustomerStatus.Active, CreatedAt = new DateTimeOffset(2024, 03, 18, 9, 0, 0, TimeSpan.Zero) },
        new() { Id = "CUST000006", Name = "Mikhail Orlov", Email = "m.orlov@example.com", Country = "CH", KycStatus = KycStatus.Rejected, Status = CustomerStatus.Closed, CreatedAt = new DateTimeOffset(2022, 11, 30, 9, 0, 0, TimeSpan.Zero) },
        new() { Id = "CUST000007", Name = "Isabella Conti", Email = "isabella.conti@example.it", Country = "IT", KycStatus = KycStatus.Verified, Status = CustomerStatus.Active, CreatedAt = new DateTimeOffset(2023, 09, 12, 9, 0, 0, TimeSpan.Zero) },
        new() { Id = "CUST000008", Name = "Robert Keller", Email = "robert.keller@example.lu", Country = "LU", KycStatus = KycStatus.Pending, Status = CustomerStatus.Active, CreatedAt = new DateTimeOffset(2024, 06, 02, 9, 0, 0, TimeSpan.Zero) },
    ];

    public static async Task<int> SeedAsync(CustomersDbContext db, CancellationToken cancellationToken = default)
    {
        var existingIds = (await db.Customers
            .Select(c => c.Id)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var newCustomers = SampleCustomers
            .Where(c => !existingIds.Contains(c.Id))
            .Select(c => c.ToEntity())
            .ToList();

        if (newCustomers.Count == 0)
            return 0;

        db.Customers.AddRange(newCustomers);
        await db.SaveChangesAsync(cancellationToken);
        return newCustomers.Count;
    }
}
