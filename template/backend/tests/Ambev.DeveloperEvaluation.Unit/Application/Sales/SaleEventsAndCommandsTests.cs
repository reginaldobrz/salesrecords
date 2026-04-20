using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Tests for Sale-related events, command Validate() methods, and value objects.
/// </summary>
public class SaleEventsAndCommandsTests
{
    // ────────────────────────── Event Tests ──────────────────────────

    [Fact(DisplayName = "Given valid data When create SaleCreatedEvent Then properties are set correctly")]
    public void Given_ValidData_When_CreateSaleCreatedEvent_Then_PropertiesAreSet()
    {
        // Given
        var saleId = Guid.NewGuid();
        const int saleNumber = 42;
        var occurredAt = DateTime.UtcNow;

        // When
        var evt = new SaleCreatedEvent
        {
            SaleId = saleId,
            SaleNumber = saleNumber,
            OccurredAt = occurredAt
        };

        // Then
        evt.SaleId.Should().Be(saleId);
        evt.SaleNumber.Should().Be(saleNumber);
        evt.OccurredAt.Should().Be(occurredAt);
    }

    [Fact(DisplayName = "Given valid data When create SaleCancelledEvent Then properties are set correctly")]
    public void Given_ValidData_When_CreateSaleCancelledEvent_Then_PropertiesAreSet()
    {
        // Given
        var saleId = Guid.NewGuid();
        const int saleNumber = 7;
        var occurredAt = DateTime.UtcNow;

        // When
        var evt = new SaleCancelledEvent
        {
            SaleId = saleId,
            SaleNumber = saleNumber,
            OccurredAt = occurredAt
        };

        // Then
        evt.SaleId.Should().Be(saleId);
        evt.SaleNumber.Should().Be(saleNumber);
        evt.OccurredAt.Should().Be(occurredAt);
    }

    [Fact(DisplayName = "Given valid data When create ItemCancelledEvent Then properties are set correctly")]
    public void Given_ValidData_When_CreateItemCancelledEvent_Then_PropertiesAreSet()
    {
        // Given
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var productName = "Widget A";
        var occurredAt = DateTime.UtcNow;

        // When
        var evt = new ItemCancelledEvent
        {
            SaleId = saleId,
            SaleItemId = itemId,
            ProductName = productName,
            OccurredAt = occurredAt
        };

        // Then
        evt.SaleId.Should().Be(saleId);
        evt.SaleItemId.Should().Be(itemId);
        evt.ProductName.Should().Be(productName);
        evt.OccurredAt.Should().Be(occurredAt);
    }

    [Fact(DisplayName = "Given valid data When create SaleModifiedEvent Then properties are set correctly")]
    public void Given_ValidData_When_CreateSaleModifiedEvent_Then_PropertiesAreSet()
    {
        // Given
        var saleId = Guid.NewGuid();
        const int saleNumber = 123;
        var occurredAt = DateTime.UtcNow;

        // When
        var evt = new SaleModifiedEvent
        {
            SaleId = saleId,
            SaleNumber = saleNumber,
            OccurredAt = occurredAt
        };

        // Then
        evt.SaleId.Should().Be(saleId);
        evt.SaleNumber.Should().Be(saleNumber);
        evt.OccurredAt.Should().Be(occurredAt);
    }

    // ────────────────────────── CreateSaleCommand.Validate() ──────────────────────────

    [Fact(DisplayName = "Given valid CreateSaleCommand When Validate Then IsValid is true")]
    public void Given_ValidCreateSaleCommand_When_Validate_Then_IsValid()
    {
        // Given
        var command = new CreateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Acme Corp",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch HQ",
            Items = new List<CreateSaleItemDto>
            {
                new CreateSaleItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Widget",
                    Quantity = 2,
                    UnitPrice = 10m
                }
            }
        };

        // When
        var result = command.Validate();

        // Then
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given invalid CreateSaleCommand When Validate Then IsValid is false with errors")]
    public void Given_InvalidCreateSaleCommand_When_Validate_Then_IsNotValid()
    {
        // Given — empty CustomerName violates the NotEmpty rule
        var command = new CreateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = string.Empty,
            BranchId = Guid.NewGuid(),
            BranchName = "Branch HQ"
        };

        // When
        var result = command.Validate();

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty(); // also forces enumeration of the deferred cast
        _ = result.Errors.ToList(); // enumerate to cover the (ValidationErrorDetail) cast line
    }

    // ────────────────────────── UpdateSaleCommand.Validate() ──────────────────────────
    [Fact(DisplayName = "Given valid UpdateSaleCommand When Validate Then IsValid is true")]
    public void Given_ValidUpdateSaleCommand_When_Validate_Then_IsValid()
    {
        // Given
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Acme Corp",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch HQ",
            Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Widget",
                    Quantity = 2,
                    UnitPrice = 10m
                }
            }
        };

        // When
        var result = command.Validate();

        // Then
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given invalid UpdateSaleCommand When Validate Then IsValid is false with errors")]
    public void Given_InvalidUpdateSaleCommand_When_Validate_Then_IsNotValid()
    {
        // Given — empty Id and empty CustomerName should fail
        var command = new UpdateSaleCommand
        {
            Id = Guid.Empty,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = string.Empty,
            BranchId = Guid.NewGuid(),
            BranchName = "Branch HQ"
        };

        // When
        var result = command.Validate();

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty(); // also forces enumeration of the deferred cast
        _ = result.Errors.ToList(); // enumerate to cover the (ValidationErrorDetail) cast line
    }
}
