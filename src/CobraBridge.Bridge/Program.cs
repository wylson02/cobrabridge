using CobraBridge.Bridge.Legacy;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Path to the legacy account master. In docker-compose this is a volume
// shared with the legacy-core container; locally it defaults to the repo copy.
var dataPath = builder.Configuration["Legacy:AccountsFile"]
    ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
                    "legacy-core", "data", "ACCOUNTS.DAT");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// The first modern endpoint backed entirely by the COBOL core.
app.MapGet("/accounts", () =>
{
    if (!File.Exists(dataPath))
        return Results.Problem($"Legacy data file not found at {dataPath}");

    var accounts = FixedWidthAccountParser.ParseFile(File.ReadLines(dataPath));
    return Results.Ok(accounts);
});

app.MapGet("/accounts/{id}", (string id) =>
{
    if (!File.Exists(dataPath))
        return Results.Problem($"Legacy data file not found at {dataPath}");

    var match = FixedWidthAccountParser
        .ParseFile(File.ReadLines(dataPath))
        .FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));

    return match is null ? Results.NotFound() : Results.Ok(match);
});

app.Run();
