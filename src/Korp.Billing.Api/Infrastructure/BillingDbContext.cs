using Korp.Billing.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Korp.Billing.Api.Infrastructure;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options)
    : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
}
