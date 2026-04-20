using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validates invariants on a <see cref="Sale"/> aggregate.
/// </summary>
public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(s => s.SaleDate)
            .NotEqual(default(DateTime))
            .WithMessage("Sale date must be provided.");

        RuleFor(s => s.CustomerId)
            .NotEqual(Guid.Empty)
            .WithMessage("Customer identifier must be provided.");

        RuleFor(s => s.CustomerName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Customer name must be provided and cannot exceed 100 characters.");

        RuleFor(s => s.BranchId)
            .NotEqual(Guid.Empty)
            .WithMessage("Branch identifier must be provided.");

        RuleFor(s => s.BranchName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Branch name must be provided and cannot exceed 100 characters.");

        RuleFor(s => s.Items)
            .NotEmpty()
            .WithMessage("A sale must contain at least one item.");
    }
}
