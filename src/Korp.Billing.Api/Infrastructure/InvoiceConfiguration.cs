using Korp.Billing.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Korp.Billing.Api.Infrastructure;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(invoice => invoice.Id);
        builder.HasIndex(invoice => invoice.Number).IsUnique();
        builder.Property(invoice => invoice.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(invoice => invoice.LastProcessingError).HasMaxLength(500);
        builder.HasMany(invoice => invoice.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(invoice => invoice.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
