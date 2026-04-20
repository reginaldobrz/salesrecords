using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Notification raised after a sale is successfully created.
/// Handlers may log, audit, or integrate with external systems.
/// </summary>
public class SaleCreatedEvent : INotification
{
    public Guid SaleId { get; init; }
    public int SaleNumber { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
