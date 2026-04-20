namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Result returned after a sale is successfully created.
/// </summary>
public class CreateSaleResult
{
    public Guid Id { get; set; }
    public int SaleNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public List<CreateSaleItemResult> Items { get; set; } = new();
}

/// <summary>Line item result within a <see cref="CreateSaleResult"/>.</summary>
public class CreateSaleItemResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
}
