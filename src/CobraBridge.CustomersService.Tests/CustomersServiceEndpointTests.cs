using System.Net;
using CobraBridge.CustomersService.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CobraBridge.CustomersService.Tests;

public class CustomersServiceEndpointTests
{
    // Swaps the real Npgsql-backed CustomersDbContext for an isolated EF
    // Core InMemory database — no real Postgres needed. The service seeds
    // its fixed sample dataset on startup regardless of provider.
    private static WebApplicationFactory<Program> CreateFactory()
    {
        var dbName = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CustomersDbContext>>();
                services.AddDbContext<CustomersDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        Assert.Contains("healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetCustomers_ReturnsSeededCustomers_WithReadableEnums()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/customers");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"id\":\"CUST000001\"", body);
        Assert.Contains("\"kycStatus\":\"Verified\"", body);
        Assert.Contains("\"status\":\"Active\"", body);
    }

    [Fact]
    public async Task GetCustomerById_ExistingId_ReturnsThatCustomer()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/customers/CUST000003");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("Anna Mueller", body);
    }

    [Fact]
    public async Task GetCustomerById_UnknownId_ReturnsNotFound()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/customers/DOES-NOT-EXIST");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomers_FilteredByKycStatus_ReturnsOnlyMatchingCustomers()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/customers?kycStatus=Rejected");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"kycStatus\":\"Rejected\"", body);
        Assert.DoesNotContain("\"kycStatus\":\"Verified\"", body);
        Assert.DoesNotContain("\"kycStatus\":\"Pending\"", body);
    }

    [Fact]
    public async Task GetCustomers_FilteredByKycStatusCaseInsensitive_StillMatches()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/customers?kycStatus=verified");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"kycStatus\":\"Verified\"", body);
    }

    [Fact]
    public async Task GetCustomers_UnknownKycStatus_ReturnsBadRequest()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/customers?kycStatus=NotARealStatus");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
