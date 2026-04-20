using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="SaleItemValidator"/>.
/// </summary>
public class SaleItemValidatorTests
{
    private readonly SaleItemValidator _validator = new SaleItemValidator();

    private static SaleItem ValidItem() => new SaleItem
    {
        ProductId = Guid.NewGuid(),
        ProductName = "Valid Product",
        Quantity = 5,
        UnitPrice = 10m
    };

    [Fact(DisplayName = "Given valid sale item When validate Then result is valid")]
    public void Given_ValidSaleItem_When_Validate_Then_ResultIsValid()
    {
        // Given
        var item = ValidItem();

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given empty ProductId When validate Then validation fails")]
    public void Given_EmptyProductId_When_Validate_Then_ValidationFails()
    {
        // Given
        var item = ValidItem();
        item.ProductId = Guid.Empty;

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaleItem.ProductId));
    }

    [Fact(DisplayName = "Given empty ProductName When validate Then validation fails")]
    public void Given_EmptyProductName_When_Validate_Then_ValidationFails()
    {
        // Given
        var item = ValidItem();
        item.ProductName = string.Empty;

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaleItem.ProductName));
    }

    [Fact(DisplayName = "Given ProductName exceeding 150 chars When validate Then validation fails")]
    public void Given_LongProductName_When_Validate_Then_ValidationFails()
    {
        // Given
        var item = ValidItem();
        item.ProductName = new string('A', 151);

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaleItem.ProductName));
    }

    [Fact(DisplayName = "Given Quantity of 0 When validate Then validation fails")]
    public void Given_QuantityZero_When_Validate_Then_ValidationFails()
    {
        // Given
        var item = ValidItem();
        item.Quantity = 0;

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaleItem.Quantity));
    }

    [Fact(DisplayName = "Given Quantity of 21 When validate Then validation fails")]
    public void Given_Quantity21_When_Validate_Then_ValidationFails()
    {
        // Given
        var item = ValidItem();
        item.Quantity = 21;

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaleItem.Quantity));
    }

    [Fact(DisplayName = "Given UnitPrice of 0 When validate Then validation fails")]
    public void Given_UnitPriceZero_When_Validate_Then_ValidationFails()
    {
        // Given
        var item = ValidItem();
        item.UnitPrice = 0m;

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaleItem.UnitPrice));
    }

    [Fact(DisplayName = "Given negative UnitPrice When validate Then validation fails")]
    public void Given_NegativeUnitPrice_When_Validate_Then_ValidationFails()
    {
        // Given
        var item = ValidItem();
        item.UnitPrice = -1m;

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaleItem.UnitPrice));
    }
}
