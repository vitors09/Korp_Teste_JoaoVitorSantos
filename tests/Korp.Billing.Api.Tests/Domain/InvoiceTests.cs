using Korp.Billing.Api.Domain;

namespace Korp.Billing.Api.Tests.Domain;

public sealed class InvoiceTests
{
    [Fact]
    public void Create_StartsOpenWithSequentialNumberAndItems()
    {
        var products = new[]
        {
            new ProductSnapshot(Guid.NewGuid(), "PROD-01", "Produto", 2)
        };

        var invoice = Invoice.Create(7, products);

        Assert.Equal(7, invoice.Number);
        Assert.Equal(InvoiceStatus.Open, invoice.Status);
        Assert.Single(invoice.Items);
        Assert.Null(invoice.ClosedAtUtc);
    }

    [Fact]
    public void Close_IsIdempotent()
    {
        var invoice = Invoice.Create(1,
        [
            new ProductSnapshot(Guid.NewGuid(), "PROD-01", "Produto", 1)
        ]);

        invoice.Close();
        var firstClosedAt = invoice.ClosedAtUtc;
        invoice.Close();

        Assert.Equal(InvoiceStatus.Closed, invoice.Status);
        Assert.Equal(firstClosedAt, invoice.ClosedAtUtc);
    }
}
