using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CobraBridge.Gateway.Tests;

public class GatewayTests
{
    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", body);
    }

    // Proves the actual YARP behavior that matters: by default (AccountsSource
    // unset -> "legacy"), a request to /api/accounts/{id} is routed to the
    // Bridge with the "/api" prefix stripped. Backed by a real (fake) HTTP
    // server, not a mock of YARP itself.
    [Fact]
    public async Task ApiAccountsRoute_DefaultsToLegacy_ProxiesToBridgeAndStripsApiPrefix()
    {
        await using var fakeBridge = await FakeUpstream.StartAsync();

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Services:Bridge"] = fakeBridge.Address
                    });
                });
            });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/accounts/ACCT000002");

        response.EnsureSuccessStatusCode();
        Assert.Equal("/accounts/ACCT000002", fakeBridge.LastReceivedPath);
    }

    // The Strangler switch itself: flipping AccountsSource to "modern"
    // re-points the same /api/accounts route at AccountsService instead of
    // the Bridge — no route changes, no client-visible difference.
    [Fact]
    public async Task ApiAccountsRoute_WhenAccountsSourceIsModern_ProxiesToAccountsService()
    {
        await using var fakeAccountsService = await FakeUpstream.StartAsync();

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["AccountsSource"] = "modern",
                        ["Services:AccountsService"] = fakeAccountsService.Address
                    });
                });
            });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/accounts");

        response.EnsureSuccessStatusCode();
        Assert.Equal("/accounts", fakeAccountsService.LastReceivedPath);
    }

    // Customers has no Strangler switch — net-new capability, one backend.
    // Smoke test: the route reaches CustomersService with "/api" stripped,
    // same as the accounts routes.
    [Fact]
    public async Task ApiCustomersRoute_ProxiesToCustomersService_AndStripsApiPrefix()
    {
        await using var fakeCustomersService = await FakeUpstream.StartAsync();

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ReverseProxy:Clusters:customers-svc:Destinations:d1:Address"] = fakeCustomersService.Address
                    });
                });
            });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/customers/CUST000003");

        response.EnsureSuccessStatusCode();
        Assert.Equal("/customers/CUST000003", fakeCustomersService.LastReceivedPath);
    }

    [Theory]
    [InlineData(null, "legacy")]
    [InlineData("modern", "modern")]
    public async Task MigrationStatus_ReflectsConfiguredAccountsSource(string? configuredSource, string expectedSource)
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                if (configuredSource is not null)
                {
                    builder.ConfigureAppConfiguration((_, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["AccountsSource"] = configuredSource
                        });
                    });
                }
            });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/_migration/status");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains($"\"accountsSource\":\"{expectedSource}\"", body);
    }

    // A minimal Kestrel host standing in for whichever backend is under
    // test, so these tests verify real proxying instead of asserting
    // against YARP's config model.
    private sealed class FakeUpstream : IAsyncDisposable
    {
        private readonly WebApplication _app;
        public string Address { get; private set; } = string.Empty;
        public string? LastReceivedPath { get; private set; }

        private FakeUpstream(WebApplication app) => _app = app;

        public static async Task<FakeUpstream> StartAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            var app = builder.Build();

            var upstream = new FakeUpstream(app);
            app.MapGet("/{**catchAll}", (HttpContext ctx) =>
            {
                upstream.LastReceivedPath = ctx.Request.Path.Value;
                return Results.Json(Array.Empty<object>());
            });

            await app.StartAsync();
            upstream.Address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.First();

            return upstream;
        }

        public async ValueTask DisposeAsync() => await _app.DisposeAsync();
    }
}
