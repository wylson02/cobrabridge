using System.Text.Json.Serialization;
using CobraBridge.Bridge.Domain;
using CobraBridge.Bridge.Legacy;

var builder = WebApplication.CreateBuilder(args);

// Serialize Account/AccountStatus enums as readable text (e.g. "Checking",
// "Active") instead of their numeric backing values.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "CobraBridge Bridge API",
        Version = "v1",
        Description = "Anti-corruption layer exposing the legacy COBOL account master as JSON."
    });
    options.MapType<AccountType>(() => new Microsoft.OpenApi.OpenApiSchema { Type = Microsoft.OpenApi.JsonSchemaType.String });
    options.MapType<AccountStatus>(() => new Microsoft.OpenApi.OpenApiSchema { Type = Microsoft.OpenApi.JsonSchemaType.String });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var dataPath = ResolveAccountsFilePath(builder.Configuration);

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("GetHealth")
    .WithSummary("Liveness check")
    .WithDescription("Returns healthy when the Bridge process is up.")
    .Produces(StatusCodes.Status200OK);

// The first modern endpoint backed entirely by the COBOL core.
app.MapGet("/accounts", () =>
{
    if (!File.Exists(dataPath))
        return Results.Problem(LegacyFileNotFoundMessage(dataPath));

    var accounts = FixedWidthAccountParser.ParseFile(File.ReadLines(dataPath));
    return Results.Ok(accounts);
})
    .WithName("GetAccounts")
    .WithSummary("List all accounts")
    .WithDescription("Parses the legacy fixed-width ACCOUNTS.DAT master and returns every account as JSON.")
    .Produces<IEnumerable<Account>>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapGet("/accounts/{id}", (string id) =>
{
    if (!File.Exists(dataPath))
        return Results.Problem(LegacyFileNotFoundMessage(dataPath));

    var match = FixedWidthAccountParser
        .ParseFile(File.ReadLines(dataPath))
        .FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));

    return match is null ? Results.NotFound() : Results.Ok(match);
})
    .WithName("GetAccountById")
    .WithSummary("Get a single account")
    .WithDescription("Looks up one account by its legacy ACCT-ID (e.g. ACCT000002).")
    .Produces<Account>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();

/// <summary>
/// Resolves the path to the legacy ACCOUNTS.DAT master.
///
/// Resolution order:
///   1. Explicit configuration key "Legacy:AccountsFile" (appsettings.json,
///      command line --Legacy:AccountsFile=..., or the env var
///      Legacy__AccountsFile — ASP.NET Core maps "__" to ":" automatically).
///   2. Repo-relative default: walk up from the executable's directory
///      looking for a "legacy-core/data" folder, so the default works the
///      same whether running from bin/Debug, bin/Release, or `dotnet run`.
/// </summary>
static string ResolveAccountsFilePath(IConfiguration configuration)
{
    var configured = configuration["Legacy:AccountsFile"];
    if (!string.IsNullOrWhiteSpace(configured))
        return Path.GetFullPath(configured);

    var repoRoot = FindAncestorContaining(AppContext.BaseDirectory, Path.Combine("legacy-core", "data"));
    if (repoRoot is null)
        throw new InvalidOperationException(
            $"Could not locate the 'legacy-core/data' folder above '{AppContext.BaseDirectory}', " +
            "and no Legacy:AccountsFile override was configured. " +
            "Set it via appsettings.json, --Legacy:AccountsFile=<path>, or the Legacy__AccountsFile env var.");

    return Path.Combine(repoRoot, "legacy-core", "data", "ACCOUNTS.DAT");
}

static string? FindAncestorContaining(string startDirectory, string relativePathToFind)
{
    for (var dir = new DirectoryInfo(startDirectory); dir is not null; dir = dir.Parent)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, relativePathToFind)))
            return dir.FullName;
    }
    return null;
}

static string LegacyFileNotFoundMessage(string path) =>
    $"Legacy data file not found at '{path}'. " +
    "Override the location with the Legacy:AccountsFile configuration key " +
    "(appsettings.json, --Legacy:AccountsFile=<path>, or the Legacy__AccountsFile env var).";
