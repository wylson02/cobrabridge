using CobraBridge.CustomersService.Domain;
using CobraBridge.CustomersService.Persistence;

namespace CobraBridge.CustomersService.Tests;

public class CustomerMapperTests
{
    [Fact]
    public void ToDomain_MapsEveryField()
    {
        var createdAt = new DateTimeOffset(2023, 07, 21, 9, 0, 0, TimeSpan.Zero);
        var entity = new CustomerEntity
        {
            Id = "CUST000003",
            Name = "Anna Mueller",
            Email = "anna.mueller@example.ch",
            Country = "CH",
            KycStatus = KycStatus.Verified,
            Status = CustomerStatus.Active,
            CreatedAt = createdAt
        };

        var customer = entity.ToDomain();

        Assert.Equal(entity.Id, customer.Id);
        Assert.Equal(entity.Name, customer.Name);
        Assert.Equal(entity.Email, customer.Email);
        Assert.Equal(entity.Country, customer.Country);
        Assert.Equal(entity.KycStatus, customer.KycStatus);
        Assert.Equal(entity.Status, customer.Status);
        Assert.Equal(createdAt, customer.CreatedAt);
    }

    [Fact]
    public void ToEntity_RoundTripsThroughToDomain_PreservesAllFields()
    {
        var original = new Customer
        {
            Id = "CUST000006",
            Name = "Mikhail Orlov",
            Email = "m.orlov@example.com",
            Country = "CH",
            KycStatus = KycStatus.Rejected,
            Status = CustomerStatus.Closed,
            CreatedAt = new DateTimeOffset(2022, 11, 30, 9, 0, 0, TimeSpan.Zero)
        };

        var roundTripped = original.ToEntity().ToDomain();

        Assert.Equal(original, roundTripped);
    }
}
