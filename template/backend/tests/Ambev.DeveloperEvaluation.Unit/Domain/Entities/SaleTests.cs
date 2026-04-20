using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Sale"/> aggregate root and <see cref="SaleItem"/> discount logic.
/// </summary>
public class SaleTests
{
    // ────────────────────────── Discount Tier Tests ──────────────────────────

    [Fact(DisplayName = "Given quantity below 4 When CalculateDiscount Then discount is 0%")]
    public void Given_QuantityBelow4_When_CalculateDiscount_Then_NoDiscount()
    {
        // Given
        var item = SaleTestData.GenerateItem(quantity: 3, unitPrice: 100m);

        // When
        item.CalculateDiscount();

        // Then
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(300m);
    }

    [Fact(DisplayName = "Given quantity of exactly 4 When CalculateDiscount Then discount is 10%")]
    public void Given_QuantityOf4_When_CalculateDiscount_Then_10PercentDiscount()
    {
        // Given
        var item = SaleTestData.GenerateItem(quantity: 4, unitPrice: 100m);

        // When
        item.CalculateDiscount();

        // Then
        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(360m); // 4 * 100 * 0.90
    }

    [Fact(DisplayName = "Given quantity of 9 When CalculateDiscount Then discount is 10%")]
    public void Given_QuantityOf9_When_CalculateDiscount_Then_10PercentDiscount()
    {
        // Given
        var item = SaleTestData.GenerateItem(quantity: 9, unitPrice: 100m);

        // When
        item.CalculateDiscount();

        // Then
        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(810m); // 9 * 100 * 0.90
    }

    [Fact(DisplayName = "Given quantity of 10 When CalculateDiscount Then discount is 20%")]
    public void Given_QuantityOf10_When_CalculateDiscount_Then_20PercentDiscount()
    {
        // Given
        var item = SaleTestData.GenerateItem(quantity: 10, unitPrice: 100m);

        // When
        item.CalculateDiscount();

        // Then
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(800m); // 10 * 100 * 0.80
    }

    [Fact(DisplayName = "Given quantity of 20 When CalculateDiscount Then discount is 20%")]
    public void Given_QuantityOf20_When_CalculateDiscount_Then_20PercentDiscount()
    {
        // Given
        var item = SaleTestData.GenerateItem(quantity: 20, unitPrice: 50m);

        // When
        item.CalculateDiscount();

        // Then
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(800m); // 20 * 50 * 0.80
    }

    [Fact(DisplayName = "Given quantity above 20 When CalculateDiscount Then throws DomainException")]
    public void Given_QuantityAbove20_When_CalculateDiscount_Then_ThrowsDomainException()
    {
        // Given
        var item = SaleTestData.GenerateItem(quantity: 21, unitPrice: 100m);

        // When
        var act = () => item.CalculateDiscount();

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("*20 identical items*");
    }

    // ────────────────────────── Sale Behaviour Tests ──────────────────────────

    [Fact(DisplayName = "Given active sale When Cancel Then IsCancelled is true")]
    public void Given_ActiveSale_When_Cancel_Then_IsCancelledIsTrue()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale();

        // When
        sale.Cancel();

        // Then
        sale.IsCancelled.Should().BeTrue();
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given sale with items When CalculateTotalAmount Then total is sum of active items")]
    public void Given_SaleWithItems_When_CalculateTotalAmount_Then_TotalIsSumOfActiveItems()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(); // 1 item qty=2, price=100 → total=200

        var cancelledItem = SaleTestData.GenerateItem(quantity: 1, unitPrice: 50m);
        cancelledItem.CalculateDiscount();
        cancelledItem.IsCancelled = true;
        cancelledItem.SaleId = sale.Id;

        // When
        sale.CalculateTotalAmount();

        // Then — the cancelled item must not contribute
        sale.TotalAmount.Should().Be(sale.Items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount));
    }

    [Fact(DisplayName = "Given sale When CancelItem with valid id Then item is cancelled and total recalculated")]
    public void Given_Sale_When_CancelItem_Then_ItemCancelledAndTotalRecalculated()
    {
        // Given
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 1,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Acme Corp",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch A"
        };

        var item1 = SaleTestData.GenerateItem(quantity: 2, unitPrice: 100m);
        var item2 = SaleTestData.GenerateItem(quantity: 3, unitPrice: 50m);
        sale.AddItem(item1);
        sale.AddItem(item2);
        sale.CalculateTotalAmount();

        var initialTotal = sale.TotalAmount;

        // When
        sale.CancelItem(item1.Id);

        // Then
        item1.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().BeLessThan(initialTotal);
    }

    [Fact(DisplayName = "Given sale When CancelItem with unknown id Then throws DomainException")]
    public void Given_Sale_When_CancelItemWithUnknownId_Then_ThrowsDomainException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale();
        var unknownId = Guid.NewGuid();

        // When
        var act = () => sale.CancelItem(unknownId);

        // Then
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Given valid sale When Validate Then result is valid")]
    public void Given_ValidSale_When_Validate_Then_ResultIsValid()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale();

        // When
        var result = sale.Validate();

        // Then
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given invalid sale When Validate Then result is not valid and errors are populated")]
    public void Given_InvalidSale_When_Validate_Then_ResultIsNotValid()
    {
        // Given — sale with empty CustomerName triggers SaleValidator rule
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 1,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = string.Empty, // violates NotEmpty rule
            BranchId = Guid.NewGuid(),
            BranchName = "Branch A"
        };

        // When
        var result = sale.Validate();

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Given sale with items When ReplaceItems Then old items are cleared and new ones set")]
    public void Given_SaleWithItems_When_ReplaceItems_Then_ItemsAreReplaced()
    {
        // Given
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 1,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Acme Corp",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch A"
        };

        var originalItem = SaleTestData.GenerateItem(quantity: 2, unitPrice: 50m);
        sale.AddItem(originalItem);
        sale.Items.Should().HaveCount(1);

        var newItem1 = SaleTestData.GenerateItem(quantity: 5, unitPrice: 20m);
        newItem1.CalculateDiscount();
        var newItem2 = SaleTestData.GenerateItem(quantity: 3, unitPrice: 30m);
        newItem2.CalculateDiscount();

        // When
        sale.ReplaceItems(new[] { newItem1, newItem2 });

        // Then
        sale.Items.Should().HaveCount(2);
        sale.Items.Should().AllSatisfy(i => i.SaleId.Should().Be(sale.Id));
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given sale item When Sale navigation property set Then getter returns it")]
    public void Given_SaleItem_When_SaleNavigationSet_Then_GetterReturnsIt()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleTestData.GenerateItem(quantity: 2, unitPrice: 10m);

        // When
        item.Sale = sale;

        // Then
        item.Sale.Should().BeSameAs(sale);
    }
}
