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

    // Proves the actual YARP behavior that matters: a request to
    // /api/accounts/{id} is routed to the "bridge" cluster with the
    // "/api" prefix stripped, so the upstream sees its native /accounts
    // path. Backed by a real (fake) HTTP server, not a mock of YARP itself.
    [Fact]
    public async Task ApiAccountsRoute_ProxiesToBridgeCluster_AndStripsApiPrefix()
    {
        await using var fakeBridge = await FakeUpstream.StartAsync();

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ReverseProxy:Clusters:bridge:Destinations:bridge1:Address"] = fakeBridge.Address
                    });
                });
            });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/accounts/ACCT000002");

        response.EnsureSuccessStatusCode();
        Assert.Equal("/accounts/ACCT000002", fakeBridge.LastReceivedPath);
    }

    // A minimal Kestrel host standing in for the Bridge, so the test
    // verifies real proxying instead of asserting against YARP's config model.
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
