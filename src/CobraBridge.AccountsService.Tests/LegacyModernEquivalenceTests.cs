using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CobraBridge.AccountsService.Persistence;
using CobraBridge.Domain;
using CobraBridge.Domain.Legacy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CobraBridge.AccountsService.Tests;

/// <summary>
/// Proves the actual Strangler claim: for the same legacy source data, what
/// the Bridge would parse directly and what AccountsService serves after
/// migrating that data into PostgreSQL are the same accounts. This is the
/// same equivalence demonstrated manually via curl against the gateway,
/// pinned here as an automated test against a fixture file.
/// </summary>
public class LegacyModernEquivalenceTests
{
    [Fact]
    public async Task ModernAccountsApi_AfterSeedingFromLegacyFile_MatchesDirectLegacyParse()
    {
        var legacyFile = LegacyFixture.WriteSampleFile();
        try
        {
            // "Legacy view": exactly what the Bridge serves — direct parse, no DB.
            var legacyAccounts = FixedWidthAccountParser
                .ParseFile(File.ReadLines(legacyFile))
                .OrderBy(a => a.Id)
                .ToList();

            // "Modern view": same file, migrated into Postgres (InMemory here),
            // served through AccountsService's real HTTP endpoint.
            var dbName = Guid.NewGuid().ToString();
            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
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
            var client = factory.CreateClient();
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var modernAccounts = await client.GetFromJsonAsync<List<Account>>("/accounts", jsonOptions);

            Assert.NotNull(modernAccounts);
            Assert.Equal(legacyAccounts, modernAccounts!.OrderBy(a => a.Id).ToList());
        }
        finally
        {
            File.Delete(legacyFile);
        }
    }
}
