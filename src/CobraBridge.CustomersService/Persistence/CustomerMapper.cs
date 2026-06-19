using CobraBridge.CustomersService.Domain;

namespace CobraBridge.CustomersService.Persistence;

public static class CustomerMapper
{
    public static Customer ToDomain(this CustomerEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Email = entity.Email,
        Country = entity.Country,
        KycStatus = entity.KycStatus,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt
    };

    public static CustomerEntity ToEntity(this Customer customer) => new()
    {
        Id = customer.Id,
        Name = customer.Name,
        Email = customer.Email,
        Country = customer.Country,
        KycStatus = customer.KycStatus,
        Status = customer.Status,
        CreatedAt = customer.CreatedAt
    };
}
