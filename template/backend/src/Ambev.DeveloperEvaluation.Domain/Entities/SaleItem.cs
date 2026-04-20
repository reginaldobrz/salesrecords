using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents an individual item within a sale.
/// Contains quantity-based discount business logic.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>The sale this item belongs to.</summary>
    public Guid SaleId { get; set; }

    /// <summary>External identity: product identifier from the product domain.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Denormalized product name at the time of sale.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Number of units purchased.</summary>
    public int Quantity { get; set; }

    /// <summary>Price per unit at the time of sale.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Discount rate applied (0.0 – 0.20). Calculated from quantity tiers.</summary>
    public decimal Discount { get; set; }

    /// <summary>Calculated line total: Quantity × UnitPrice × (1 − Discount).</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Whether this item has been individually cancelled.</summary>
    public bool IsCancelled { get; set; }

    /// <summary>Navigation property back to the owning sale.</summary>
    public Sale? Sale { get; set; }

    /// <summary>
    /// Applies the quantity-based discount tier and recalculates TotalAmount.
    /// </summary>
    /// <exception cref="DomainException">Thrown when quantity exceeds 20.</exception>
    public void CalculateDiscount()
    {
        if (Quantity > 20)
            throw new DomainException("It is not possible to sell above 20 identical items.");

        Discount = Quantity switch
        {
            >= 10 => 0.20m,
            >= 4  => 0.10m,
            _     => 0.00m
        };

        TotalAmount = Quantity * UnitPrice * (1 - Discount);
    }
}
