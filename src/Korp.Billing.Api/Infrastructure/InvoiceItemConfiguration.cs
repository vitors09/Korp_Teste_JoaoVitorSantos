using Korp.Billing.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Billing.Api.Infrastructure;

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProductCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.ProductDescription).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Quantity).IsRequired();
    }
}
