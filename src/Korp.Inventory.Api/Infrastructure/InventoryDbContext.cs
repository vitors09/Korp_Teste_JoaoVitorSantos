using Korp.Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Korp.Inventory.Api.Infrastructure;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockOperation> StockOperations => Set<StockOperation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
