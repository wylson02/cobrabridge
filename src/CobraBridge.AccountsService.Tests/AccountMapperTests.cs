using CobraBridge.AccountsService.Persistence;
using CobraBridge.Domain;

namespace CobraBridge.AccountsService.Tests;

public class AccountMapperTests
{
    [Fact]
    public void ToDomain_MapsEveryFieldAndComputesBalance()
    {
        var entity = new AccountEntity
        {
            Id = "ACCT000002",
            Name = "ANNA MUELLER",
            Type = AccountType.Savings,
            BalanceCents = 8450050,
            Status = AccountStatus.Active
        };

        var account = entity.ToDomain();

        Assert.Equal(entity.Id, account.Id);
        Assert.Equal(entity.Name, account.Name);
        Assert.Equal(entity.Type, account.Type);
        Assert.Equal(entity.BalanceCents, account.BalanceCents);
        Assert.Equal(entity.Status, account.Status);
        Assert.Equal(84500.50m, account.Balance);
    }

    [Fact]
    public void ToEntity_RoundTripsThroughToDomain_PreservesAllFields()
    {
        var original = new Account
        {
            Id = "ACCT000007",
            Name = "FROZEN INDUSTRIES",
            Type = AccountType.Checking,
            BalanceCents = 1234567,
            Status = AccountStatus.Frozen
        };

        var roundTripped = original.ToEntity().ToDomain();

        Assert.Equal(original, roundTripped);
    }
}
