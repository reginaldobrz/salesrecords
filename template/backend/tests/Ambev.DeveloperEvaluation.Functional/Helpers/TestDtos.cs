using System.Text.Json;

namespace Ambev.DeveloperEvaluation.Functional.Helpers;

// ─── JSON helpers ─────────────────────────────────────────────────────────────

/// <summary>
/// Shared JsonSerializerOptions for deserialization of API responses.
/// Case-insensitive matching is required because ASP.NET Core serializes
/// property names in camelCase (e.g. "id") while our DTOs use PascalCase.
/// </summary>
public static class JsonOptions
{
    public static readonly JsonSerializerOptions CaseInsensitive = new()
    {
        PropertyNameCaseInsensitive = true
    };
}

// ─── Request DTOs ────────────────────────────────────────────────────────────

public class SaleItemRequest
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateSaleRequest
{
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<SaleItemRequest> Items { get; set; } = new();
}

public class UpdateSaleRequest
{
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<SaleItemRequest> Items { get; set; } = new();
}

// ─── Response envelope ───────────────────────────────────────────────────────

public class ApiEnvelope<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

// ─── Response DTOs ───────────────────────────────────────────────────────────

public class SaleItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}

public class SaleDto
{
    public Guid Id { get; set; }
    public int SaleNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
}

public class PaginatedSalesDto
{
    /// <summary>
    /// The items in the current page — these are summary items, not full SaleDto.
    /// Matches the API's GetSalesItemResponse shape.
    /// </summary>
    public List<SalesSummaryItemDto> Data { get; set; } = new();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class SalesSummaryItemDto
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

// ─── Factories ───────────────────────────────────────────────────────────────

public static class SaleRequestFactory
{
    /// <summary>Builds a valid CreateSaleRequest with the given item quantity.</summary>
    public static CreateSaleRequest Valid(int quantity = 2, decimal unitPrice = 10m) => new()
    {
        SaleDate = DateTime.UtcNow,
        CustomerId = Guid.NewGuid(),
        CustomerName = "Functional Test Customer",
        BranchId = Guid.NewGuid(),
        BranchName = "Functional Test Branch",
        Items = new List<SaleItemRequest>
        {
            new()
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Test Product",
                Quantity = quantity,
                UnitPrice = unitPrice
            }
        }
    };

    /// <summary>Builds an UpdateSaleRequest from an existing CreateSaleRequest.</summary>
    public static UpdateSaleRequest ToUpdate(CreateSaleRequest original, int newQuantity) => new()
    {
        SaleDate = original.SaleDate,
        CustomerId = original.CustomerId,
        CustomerName = original.CustomerName + " (updated)",
        BranchId = original.BranchId,
        BranchName = original.BranchName,
        Items = new List<SaleItemRequest>
        {
            new()
            {
                ProductId = original.Items[0].ProductId,
                ProductName = original.Items[0].ProductName,
                Quantity = newQuantity,
                UnitPrice = original.Items[0].UnitPrice
            }
        }
    };
}
