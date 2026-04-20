namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>Paginated list result for sales queries.</summary>
public class GetSalesResult
{
    public IEnumerable<GetSalesItemResult> Items { get; set; } = Enumerable.Empty<GetSalesItemResult>();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class GetSalesItemResult
{
    public Guid Id { get; set; }
    public int SaleNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
}
