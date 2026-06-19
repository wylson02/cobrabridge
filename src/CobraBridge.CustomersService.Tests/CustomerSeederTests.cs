using CobraBridge.CustomersService.Persistence;
using CobraBridge.CustomersService.Seeding;
using Microsoft.EntityFrameworkCore;

namespace CobraBridge.CustomersService.Tests;

public class CustomerSeederTests
{
    private static CustomersDbContext NewInMemoryDb() => new(
        new DbContextOptionsBuilder<CustomersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task SeedAsync_InsertsTheSampleCustomers()
    {
        using var db = NewInMemoryDb();

        var inserted = await CustomerSeeder.SeedAsync(db);

        Assert.True(inserted > 0);
        Assert.Equal(inserted, await db.Customers.CountAsync());
        Assert.True(await db.Customers.AnyAsync(c => c.Id == "CUST000001"));
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_IsIdempotent()
    {
        using var db = NewInMemoryDb();

        var firstRun = await CustomerSeeder.SeedAsync(db);
        var secondRun = await CustomerSeeder.SeedAsync(db);
        var totalAfter = await db.Customers.CountAsync();

        Assert.True(firstRun > 0);
        Assert.Equal(0, secondRun);
        Assert.Equal(firstRun, totalAfter);
    }
}
