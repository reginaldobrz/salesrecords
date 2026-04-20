using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validates invariants on a <see cref="SaleItem"/>.
/// </summary>
public class SaleItemValidator : AbstractValidator<SaleItem>
{
    public SaleItemValidator()
    {
        RuleFor(i => i.ProductId)
            .NotEqual(Guid.Empty)
            .WithMessage("Product identifier must be provided.");

        RuleFor(i => i.ProductName)
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("Product name must be provided and cannot exceed 150 characters.");

        RuleFor(i => i.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(20).WithMessage("It is not possible to sell above 20 identical items.");

        RuleFor(i => i.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than zero.");
    }
}
