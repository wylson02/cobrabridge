using CobraBridge.Domain;

namespace CobraBridge.AccountsService.Persistence;

public static class AccountMapper
{
    public static Account ToDomain(this AccountEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Type = entity.Type,
        BalanceCents = entity.BalanceCents,
        Status = entity.Status
    };

    public static AccountEntity ToEntity(this Account account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Type = account.Type,
        BalanceCents = account.BalanceCents,
        Status = account.Status
    };
}
