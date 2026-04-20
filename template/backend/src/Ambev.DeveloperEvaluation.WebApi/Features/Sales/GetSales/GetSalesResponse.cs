namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;

public class GetSalesResponse
{
    public IEnumerable<GetSalesItemResponse> Data { get; set; } = Enumerable.Empty<GetSalesItemResponse>();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class GetSalesItemResponse
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
