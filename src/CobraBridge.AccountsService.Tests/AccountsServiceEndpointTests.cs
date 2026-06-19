using System.Linq;
using CobraBridge.AccountsService.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CobraBridge.AccountsService.Tests;

public class AccountsServiceEndpointTests
{
    // Swaps the real Npgsql-backed AccountsDbContext for an isolated EF Core
    // InMemory database, and points the service's startup legacy seed at a
    // disposable fixture file — so these tests need neither a real Postgres
    // nor the repo's real ACCOUNTS.DAT.
    private static WebApplicationFactory<Program> CreateFactory(string legacyFile)
    {
        var dbName = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Legacy:AccountsFile"] = legacyFile
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AccountsDbContext>>();
                services.AddDbContext<AccountsDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var legacyFile = LegacyFixture.WriteSampleFile();
        try
        {
            await using var factory = CreateFactory(legacyFile);
            var client = factory.CreateClient();

            var response = await client.GetAsync("/health");

            response.EnsureSuccessStatusCode();
            Assert.Contains("healthy", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            File.Delete(legacyFile);
        }
    }

    [Fact]
    public async Task GetAccounts_ReturnsSeededLegacyAccounts_WithReadableEnumsAndDecimalBalance()
    {
        var legacyFile = LegacyFixture.WriteSampleFile();
        try
        {
            await using var factory = CreateFactory(legacyFile);
            var client = factory.CreateClient();

            var response = await client.GetAsync("/accounts");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            Assert.Contains("\"id\":\"ACCT000002\"", body);
            Assert.Contains("\"type\":\"Savings\"", body);
            Assert.Contains("\"status\":\"Active\"", body);
            Assert.Contains("\"balance\":84500.5", body);
        }
        finally
        {
            File.Delete(legacyFile);
        }
    }

    [Fact]
    public async Task GetAccountById_ExistingId_ReturnsThatAccount()
    {
        var legacyFile = LegacyFixture.WriteSampleFile();
        try
        {
            await using var factory = CreateFactory(legacyFile);
            var client = factory.CreateClient();

            var response = await client.GetAsync("/accounts/ACCT000002");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            Assert.Contains("ANNA MUELLER", body);
        }
        finally
        {
            File.Delete(legacyFile);
        }
    }

    [Fact]
    public async Task GetAccountById_UnknownId_ReturnsNotFound()
    {
        var legacyFile = LegacyFixture.WriteSampleFile();
        try
        {
            await using var factory = CreateFactory(legacyFile);
            var client = factory.CreateClient();

            var response = await client.GetAsync("/accounts/DOES-NOT-EXIST");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            File.Delete(legacyFile);
        }
    }
}
