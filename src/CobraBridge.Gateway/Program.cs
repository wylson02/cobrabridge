var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// The gateway's own liveness check — distinct from the proxied services.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapReverseProxy();

app.Run();

// Exposed so the test project can host this app via WebApplicationFactory<Program>.
public partial class Program { }
