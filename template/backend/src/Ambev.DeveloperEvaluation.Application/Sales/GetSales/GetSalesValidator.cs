using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

public class GetSalesValidator : AbstractValidator<GetSalesCommand>
{
    public GetSalesValidator()
    {
        RuleFor(c => c.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(c => c.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}
