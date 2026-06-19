using CobraBridge.AccountsService.Persistence;
using CobraBridge.Domain.Legacy;
using Microsoft.EntityFrameworkCore;

namespace CobraBridge.AccountsService.LegacyMigration;

/// <summary>
/// One-time data migration: reads the legacy fixed-width account master
/// (the same file the Bridge serves live) and copies every account into
/// PostgreSQL. This is the "strangle" moment for the data itself — once
/// this has run, the modern AccountsService no longer needs the COBOL core
/// to answer queries.
///
/// Idempotent: only inserts accounts whose Id isn't already in the table,
/// so re-running it (e.g. on every container start) is a safe no-op after
/// the first successful run.
/// </summary>
public static class LegacySeeder
{
    public static async Task<int> SeedFromLegacyFileAsync(
        AccountsDbContext db, string legacyFilePath, CancellationToken cancellationToken = default)
    {
        var legacyAccounts = FixedWidthAccountParser.ParseFile(File.ReadLines(legacyFilePath)).ToList();

        var existingIds = (await db.Accounts
            .Select(a => a.Id)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var newAccounts = legacyAccounts
            .Where(a => !existingIds.Contains(a.Id))
            .Select(a => a.ToEntity())
            .ToList();

        if (newAccounts.Count == 0)
            return 0;

        db.Accounts.AddRange(newAccounts);
        await db.SaveChangesAsync(cancellationToken);
        return newAccounts.Count;
    }
}
