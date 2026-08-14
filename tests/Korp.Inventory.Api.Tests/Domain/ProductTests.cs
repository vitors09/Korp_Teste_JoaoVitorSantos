using Korp.Inventory.Api.Domain;
using Korp.Inventory.Api.Domain.Exceptions;

namespace Korp.Inventory.Api.Tests.Domain;

public sealed class ProductTests
{
    [Fact]
    public void Create_WithValidData_NormalizesAndStoresProduct()
    {
        var product = Product.Create("  abc-01  ", "  Teclado mecânico  ", 10);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("ABC-01", product.Code);
        Assert.Equal("Teclado mecânico", product.Description);
        Assert.Equal(10, product.Balance);
    }

    [Fact]
    public void Create_WithNegativeBalance_ThrowsDomainRuleException()
    {
        var exception = Assert.Throws<DomainRuleException>(
            () => Product.Create("ABC-01", "Teclado", -1));

        Assert.Equal("O saldo inicial não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void Create_WithCodeLongerThanLimit_ThrowsDomainRuleException()
    {
        var code = new string('A', 51);

        Assert.Throws<DomainRuleException>(() => Product.Create(code, "Teclado", 1));
    }

    [Fact]
    public void Debit_WithAvailableStock_DecreasesBalance()
    {
        var product = Product.Create("ABC-01", "Teclado", 10);

        product.Debit(3);

        Assert.Equal(7, product.Balance);
    }

    [Fact]
    public void Debit_WithInsufficientStock_ThrowsAndPreservesBalance()
    {
        var product = Product.Create("ABC-01", "Teclado", 2);

        var exception = Assert.Throws<InsufficientStockException>(() => product.Debit(3));

        Assert.Equal(2, exception.AvailableBalance);
        Assert.Equal(3, exception.RequestedQuantity);
        Assert.Equal(2, product.Balance);
    }

    [Fact]
    public void Debit_WithZeroQuantity_ThrowsDomainRuleException()
    {
        var product = Product.Create("ABC-01", "Teclado", 10);

        Assert.Throws<DomainRuleException>(() => product.Debit(0));
    }
}
