using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Command to create a new sale record.
/// Implements <see cref="IRequest{TResponse}"/> to integrate with the MediatR pipeline.
/// </summary>
public class CreateSaleCommand : IRequest<CreateSaleResult>
{
    /// <summary>Date and time the sale was made.</summary>
    public DateTime SaleDate { get; set; }

    /// <summary>External identity: customer unique identifier.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Denormalized customer name at the time of sale.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>External identity: branch unique identifier.</summary>
    public Guid BranchId { get; set; }

    /// <summary>Denormalized branch name at the time of sale.</summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>Line items to include in the sale.</summary>
    public List<CreateSaleItemDto> Items { get; set; } = new();

    public ValidationResultDetail Validate()
    {
        var validator = new CreateSaleCommandValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}

/// <summary>DTO for a single line item within a CreateSaleCommand.</summary>
public class CreateSaleItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
