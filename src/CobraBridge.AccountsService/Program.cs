using System.Text.Json.Serialization;
using CobraBridge.AccountsService.LegacyMigration;
using CobraBridge.AccountsService.Persistence;
using CobraBridge.Domain.Legacy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Serialize Account/AccountStatus enums as readable text — same wire
// contract as the Bridge, so switching the gateway's traffic source is
// invisible to clients.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Dev-only default credentials — never used outside this local/demo stack.
// Database-per-service: "cobrabridge_accounts" is this service's own
// database (see docs/architecture.md). docker-compose and any real
// deployment override this via ConnectionStrings__Accounts.
var connectionString = builder.Configuration.GetConnectionString("Accounts")
    ?? "Host=localhost;Port=5432;Database=cobrabridge_accounts;Username=cobrabridge;Password=cobrabridge_dev_only_change_me";

builder.Services.AddDbContext<AccountsDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

// This project has no real-production deployment yet, so the service owns
// its own schema: apply pending migrations, then seed from the legacy
// master if one is reachable. A real prod rollout would split this into a
// separate migration step in CI/CD instead of running it on every boot.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();

    // Migrations are a relational-provider concept; tests substitute the
    // EF Core InMemory provider, which doesn't support (or need) them.
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();

    var legacyFile = ResolveLegacyFileOrNull(builder.Configuration);
    if (legacyFile is not null)
    {
        var inserted = await LegacySeeder.SeedFromLegacyFileAsync(db, legacyFile);
        app.Logger.LogInformation(
            "Legacy seed from {LegacyFile}: {Inserted} account(s) inserted.", legacyFile, inserted);
    }
    else
    {
        app.Logger.LogInformation("No legacy accounts file found; skipping legacy seed.");
    }
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/accounts", async (AccountsDbContext db) =>
{
    var accounts = await db.Accounts.OrderBy(a => a.Id).ToListAsync();
    return Results.Ok(accounts.Select(a => a.ToDomain()));
});

app.MapGet("/accounts/{id}", async (string id, AccountsDbContext db) =>
{
    var entity = await db.Accounts.FirstOrDefaultAsync(a => a.Id.ToUpper() == id.ToUpper());
    return entity is null ? Results.NotFound() : Results.Ok(entity.ToDomain());
});

app.Run();

static string? ResolveLegacyFileOrNull(IConfiguration configuration)
{
    try
    {
        var path = LegacyDataLocator.ResolveAccountsFilePath(
            configuration["Legacy:AccountsFile"], AppContext.BaseDirectory);
        return File.Exists(path) ? path : null;
    }
    catch (InvalidOperationException)
    {
        return null;
    }
}

// Exposed so the test project can host this app via WebApplicationFactory<Program>.
public partial class Program { }
