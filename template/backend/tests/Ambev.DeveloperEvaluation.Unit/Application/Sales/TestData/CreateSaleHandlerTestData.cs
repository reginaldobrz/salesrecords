using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;

/// <summary>
/// Provides Bogus-based test data generators for <see cref="CreateSaleCommand"/> scenarios.
/// </summary>
public static class CreateSaleHandlerTestData
{
    private static readonly Faker<CreateSaleItemDto> ItemFaker = new Faker<CreateSaleItemDto>()
        .RuleFor(i => i.ProductId, _ => Guid.NewGuid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 3))
        .RuleFor(i => i.UnitPrice, f => Math.Round(f.Random.Decimal(5, 200), 2));

    private static readonly Faker<CreateSaleCommand> ValidCommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(c => c.SaleDate, f => f.Date.Recent(10))
        .RuleFor(c => c.CustomerId, _ => Guid.NewGuid())
        .RuleFor(c => c.CustomerName, f => f.Company.CompanyName())
        .RuleFor(c => c.BranchId, _ => Guid.NewGuid())
        .RuleFor(c => c.BranchName, f => f.Address.City())
        .RuleFor(c => c.Items, _ => ItemFaker.Generate(2));

    /// <summary>Returns a fully populated, valid <see cref="CreateSaleCommand"/>.</summary>
    public static CreateSaleCommand GenerateValidCommand() => ValidCommandFaker.Generate();

    /// <summary>Returns a command with an empty CustomerName to trigger validation failure.</summary>
    public static CreateSaleCommand GenerateInvalidCommand()
    {
        var cmd = ValidCommandFaker.Generate();
        cmd.CustomerName = string.Empty;
        return cmd;
    }

    /// <summary>Returns a domain <see cref="Sale"/> matching the given command (post-persistence stub).</summary>
    public static Sale GenerateSaleFromCommand(CreateSaleCommand command)
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = new Faker().Random.Int(1, 9999),
            SaleDate = command.SaleDate,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            BranchId = command.BranchId,
            BranchName = command.BranchName,
            IsCancelled = false
        };

        foreach (var dto in command.Items)
        {
            var item = new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice
            };
            item.CalculateDiscount();
            sale.AddItem(item);
        }

        sale.CalculateTotalAmount();
        return sale;
    }
}
