using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Notification raised when a single item within a sale is cancelled.
/// </summary>
public class ItemCancelledEvent : INotification
{
    public Guid SaleId { get; init; }
    public Guid SaleItemId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
