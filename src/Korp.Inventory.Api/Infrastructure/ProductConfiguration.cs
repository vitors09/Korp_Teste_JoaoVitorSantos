using Korp.Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Inventory.Api.Infrastructure;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(product => product.Code)
            .IsUnique();

        builder.Property(product => product.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Balance)
            .IsRequired();

        builder.Property(product => product.Version)
            .IsConcurrencyToken()
            .IsRequired();
    }
}
