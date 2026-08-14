namespace Korp.Inventory.Api.Domain;

public sealed class StockOperation
{
    private StockOperation()
    {
    }

    private StockOperation(Guid idempotencyKey)
    {
        Id = Guid.NewGuid();
        IdempotencyKey = idempotencyKey;
        ProcessedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid IdempotencyKey { get; private set; }
    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static StockOperation Create(Guid idempotencyKey) => new(idempotencyKey);
}
