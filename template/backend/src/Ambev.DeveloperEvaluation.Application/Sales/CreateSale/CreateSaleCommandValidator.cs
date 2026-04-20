using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Validates the <see cref="CreateSaleCommand"/> before the handler executes.
/// </summary>
public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(c => c.SaleDate)
            .NotEqual(default(DateTime))
            .WithMessage("Sale date must be provided.");

        RuleFor(c => c.CustomerId)
            .NotEqual(Guid.Empty)
            .WithMessage("Customer identifier must be provided.");

        RuleFor(c => c.CustomerName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.BranchId)
            .NotEqual(Guid.Empty)
            .WithMessage("Branch identifier must be provided.");

        RuleFor(c => c.BranchName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.Items)
            .NotEmpty()
            .WithMessage("A sale must contain at least one item.");

        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEqual(Guid.Empty)
                .WithMessage("Product identifier must be provided for every item.");

            item.RuleFor(i => i.ProductName)
                .NotEmpty()
                .MaximumLength(150);

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(20).WithMessage("It is not possible to sell above 20 identical items.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0)
                .WithMessage("Unit price must be greater than zero.");
        });
    }
}
