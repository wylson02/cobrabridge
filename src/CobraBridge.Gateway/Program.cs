var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// --- Strangler switch ---
// AccountsSource picks which backend answers /api/accounts: "legacy" (the
// Bridge, COBOL via legacy-core) or "modern" (AccountsService, PostgreSQL).
// Both speak the exact same JSON contract, so this is a pure data-source
// swap — no route, no client-visible change. Resolved after Build() (not
// while configuring builder.Services) so it sees the final configuration,
// including anything WebApplicationFactory injects for tests. YARP's
// LoadFromConfig binds reactively to the config section, so mutating it
// here still takes effect before the first request is routed.
var accountsSource = app.Configuration["AccountsSource"] ?? "legacy";
var bridgeAddress = app.Configuration["Services:Bridge"] ?? "http://localhost:5080/";
var accountsServiceAddress = app.Configuration["Services:AccountsService"] ?? "http://localhost:5100/";

var activeAccountsAddress = string.Equals(accountsSource, "modern", StringComparison.OrdinalIgnoreCase)
    ? accountsServiceAddress
    : bridgeAddress;

app.Configuration["ReverseProxy:Clusters:accounts:Destinations:accounts1:Address"] = activeAccountsAddress;

// The gateway's own liveness check — distinct from the proxied services.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Lets a future dashboard (and curl, today) see which backend is currently
// live behind /api/accounts without guessing from response shape.
app.MapGet("/api/_migration/status", () => Results.Ok(new
{
    accountsSource,
    activeBackend = activeAccountsAddress,
    description = string.Equals(accountsSource, "modern", StringComparison.OrdinalIgnoreCase)
        ? "GET /api/accounts is served by AccountsService (PostgreSQL, modern)."
        : "GET /api/accounts is served by the Bridge (COBOL via legacy-core, legacy)."
}));

app.MapReverseProxy();

app.Run();

// Exposed so the test project can host this app via WebApplicationFactory<Program>.
public partial class Program { }
