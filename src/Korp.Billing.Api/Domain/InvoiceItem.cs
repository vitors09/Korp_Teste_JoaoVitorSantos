namespace Korp.Billing.Api.Domain;

public sealed class InvoiceItem
{
    private InvoiceItem()
    {
    }

    internal InvoiceItem(
        Guid productId,
        string productCode,
        string productDescription,
        int quantity)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductCode = productCode;
        ProductDescription = productDescription;
        Quantity = quantity;
    }

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string ProductDescription { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
}
