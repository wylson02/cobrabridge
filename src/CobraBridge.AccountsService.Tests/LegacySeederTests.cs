using CobraBridge.AccountsService.LegacyMigration;
using CobraBridge.AccountsService.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CobraBridge.AccountsService.Tests;

public class LegacySeederTests
{
    private static AccountsDbContext NewInMemoryDb() => new(
        new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task SeedFromLegacyFileAsync_InsertsEveryLegacyAccount()
    {
        var legacyFile = LegacyFixture.WriteSampleFile();
        try
        {
            using var db = NewInMemoryDb();

            var inserted = await LegacySeeder.SeedFromLegacyFileAsync(db, legacyFile);

            Assert.Equal(3, inserted);
            Assert.Equal(3, await db.Accounts.CountAsync());
            Assert.True(await db.Accounts.AnyAsync(a => a.Id == "ACCT000002"));
        }
        finally
        {
            File.Delete(legacyFile);
        }
    }

    [Fact]
    public async Task SeedFromLegacyFileAsync_CalledTwice_IsIdempotent()
    {
        var legacyFile = LegacyFixture.WriteSampleFile();
        try
        {
            using var db = NewInMemoryDb();

            var firstRun = await LegacySeeder.SeedFromLegacyFileAsync(db, legacyFile);
            var secondRun = await LegacySeeder.SeedFromLegacyFileAsync(db, legacyFile);

            Assert.Equal(3, firstRun);
            Assert.Equal(0, secondRun);
            Assert.Equal(3, await db.Accounts.CountAsync());
        }
        finally
        {
            File.Delete(legacyFile);
        }
    }
}
