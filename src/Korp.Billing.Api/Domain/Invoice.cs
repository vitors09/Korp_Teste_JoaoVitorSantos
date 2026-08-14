using Korp.Billing.Api.Domain.Exceptions;

namespace Korp.Billing.Api.Domain;

public sealed class Invoice
{
    private readonly List<InvoiceItem> items = [];

    private Invoice()
    {
    }

    private Invoice(int number, IEnumerable<InvoiceItem> invoiceItems)
    {
        Id = Guid.NewGuid();
        Number = number;
        Status = InvoiceStatus.Open;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        items.AddRange(invoiceItems);
    }

    public Guid Id { get; private set; }
    public int Number { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public string? LastProcessingError { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => items;

    public static Invoice Create(int number, IEnumerable<ProductSnapshot> products)
    {
        if (number <= 0)
        {
            throw new BillingRuleException("A numeração da nota deve ser positiva.");
        }

        var invoiceItems = products
            .Select(product => new InvoiceItem(
                product.ProductId,
                product.Code,
                product.Description,
                product.Quantity))
            .ToList();

        if (invoiceItems.Count == 0)
        {
            throw new BillingRuleException("A nota fiscal deve possuir pelo menos um produto.");
        }

        return new Invoice(number, invoiceItems);
    }

    public void Close()
    {
        if (Status == InvoiceStatus.Closed)
        {
            return;
        }

        Status = InvoiceStatus.Closed;
        ClosedAtUtc = DateTimeOffset.UtcNow;
        LastProcessingError = null;
    }

    public void RegisterProcessingFailure(string message)
    {
        if (Status == InvoiceStatus.Open)
        {
            LastProcessingError = message;
        }
    }
}

public sealed record ProductSnapshot(
    Guid ProductId,
    string Code,
    string Description,
    int Quantity);
