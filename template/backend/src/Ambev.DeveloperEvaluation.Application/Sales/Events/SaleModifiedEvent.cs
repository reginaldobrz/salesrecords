using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Notification raised after a sale is modified (e.g. items updated or quantities changed).
/// </summary>
public class SaleModifiedEvent : INotification
{
    public Guid SaleId { get; init; }
    public int SaleNumber { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
