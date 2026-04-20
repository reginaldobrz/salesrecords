using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleRequestValidator : AbstractValidator<UpdateSaleRequest>
{
    public UpdateSaleRequestValidator()
    {
        RuleFor(r => r.SaleDate)
            .NotEqual(default(DateTime))
            .WithMessage("Sale date must be provided.");

        RuleFor(r => r.CustomerId)
            .NotEqual(Guid.Empty)
            .WithMessage("Customer identifier must be provided.");

        RuleFor(r => r.CustomerName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(r => r.BranchId)
            .NotEqual(Guid.Empty)
            .WithMessage("Branch identifier must be provided.");

        RuleFor(r => r.BranchName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(r => r.Items)
            .NotEmpty()
            .WithMessage("A sale must contain at least one item.");

        RuleForEach(r => r.Items).ChildRules(item =>
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
