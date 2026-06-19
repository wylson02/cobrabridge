using System.Text.Json.Serialization;
using CobraBridge.CustomersService.Domain;
using CobraBridge.CustomersService.Persistence;
using CobraBridge.CustomersService.Seeding;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Serialize enums as readable text — same convention as the rest of the
// system (Bridge, AccountsService), even though Customers shares no wire
// contract with either: consistency for anyone reading the API surface.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Dev-only default credentials — never used outside this local/demo stack.
// Database-per-service: this is its own "cobrabridge_customers" database on
// the same Postgres server AccountsService uses (see docs/architecture.md).
var connectionString = builder.Configuration.GetConnectionString("Customers")
    ?? "Host=localhost;Port=5432;Database=cobrabridge_customers;Username=cobrabridge;Password=cobrabridge_dev_only_change_me";

builder.Services.AddDbContext<CustomersDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

// This project has no real-production deployment yet, so the service owns
// its own schema: apply pending migrations, then seed a starter dataset.
// A real prod rollout would split this into a separate migration step in
// CI/CD instead of running it on every boot.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();

    // Migrations are a relational-provider concept; tests substitute the
    // EF Core InMemory provider, which doesn't support (or need) them.
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();

    var inserted = await CustomerSeeder.SeedAsync(db);
    app.Logger.LogInformation("Customer seed: {Inserted} customer(s) inserted.", inserted);
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/customers", async (string? kycStatus, CustomersDbContext db) =>
{
    var query = db.Customers.AsQueryable();

    if (!string.IsNullOrWhiteSpace(kycStatus))
    {
        if (!Enum.TryParse<KycStatus>(kycStatus, ignoreCase: true, out var parsedStatus))
            return Results.BadRequest(new { error = $"Unknown kycStatus '{kycStatus}'." });

        query = query.Where(c => c.KycStatus == parsedStatus);
    }

    var customers = await query.OrderBy(c => c.Id).ToListAsync();
    return Results.Ok(customers.Select(c => c.ToDomain()));
});

app.MapGet("/customers/{id}", async (string id, CustomersDbContext db) =>
{
    var entity = await db.Customers.FirstOrDefaultAsync(c => c.Id.ToUpper() == id.ToUpper());
    return entity is null ? Results.NotFound() : Results.Ok(entity.ToDomain());
});

app.Run();

// Exposed so the test project can host this app via WebApplicationFactory<Program>.
public partial class Program { }
