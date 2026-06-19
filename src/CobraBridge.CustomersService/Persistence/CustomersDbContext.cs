using Microsoft.EntityFrameworkCore;

namespace CobraBridge.CustomersService.Persistence;

public class CustomersDbContext(DbContextOptions<CustomersDbContext> options) : DbContext(options)
{
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerEntity>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasMaxLength(10);
            entity.Property(c => c.Name).HasMaxLength(100);
            entity.Property(c => c.Email).HasMaxLength(150);
            entity.Property(c => c.Country).HasMaxLength(2);
            entity.Property(c => c.KycStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
