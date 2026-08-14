using Korp.Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Inventory.Api.Infrastructure;

public sealed class StockOperationConfiguration : IEntityTypeConfiguration<StockOperation>
{
    public void Configure(EntityTypeBuilder<StockOperation> builder)
    {
        builder.ToTable("StockOperations");
        builder.HasKey(operation => operation.Id);
        builder.HasIndex(operation => operation.IdempotencyKey).IsUnique();
        builder.Property(operation => operation.ProcessedAtUtc).IsRequired();
    }
}
