using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides Bogus-backed test data generators for <see cref="Sale"/> and <see cref="SaleItem"/>.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>()
        .RuleFor(i => i.Id, _ => Guid.NewGuid())
        .RuleFor(i => i.ProductId, _ => Guid.NewGuid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 3))   // default: no discount tier
        .RuleFor(i => i.UnitPrice, f => Math.Round(f.Random.Decimal(5, 500), 2))
        .RuleFor(i => i.IsCancelled, _ => false);

    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .RuleFor(s => s.Id, _ => Guid.NewGuid())
        .RuleFor(s => s.SaleNumber, f => f.Random.Int(1, 9999))
        .RuleFor(s => s.SaleDate, f => f.Date.Recent(30))
        .RuleFor(s => s.CustomerId, _ => Guid.NewGuid())
        .RuleFor(s => s.CustomerName, f => f.Company.CompanyName())
        .RuleFor(s => s.BranchId, _ => Guid.NewGuid())
        .RuleFor(s => s.BranchName, f => f.Address.City())
        .RuleFor(s => s.IsCancelled, _ => false);

    /// <summary>Generates a valid <see cref="Sale"/> with one non-discounted item.</summary>
    public static Sale GenerateValidSale()
    {
        var sale = SaleFaker.Generate();
        var item = GenerateItem(quantity: 2);
        sale.AddItem(item);
        sale.CalculateTotalAmount();
        return sale;
    }

    /// <summary>Generates a <see cref="SaleItem"/> with a specific quantity.</summary>
    public static SaleItem GenerateItem(int quantity = 2, decimal unitPrice = 100m)
    {
        return new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = new Faker().Commerce.ProductName(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            IsCancelled = false
        };
    }
}
