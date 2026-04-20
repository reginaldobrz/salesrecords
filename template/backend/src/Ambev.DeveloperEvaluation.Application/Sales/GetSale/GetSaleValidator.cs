using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleValidator : AbstractValidator<GetSaleCommand>
{
    public GetSaleValidator()
    {
        RuleFor(c => c.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("Sale identifier must be provided.");
    }
}
