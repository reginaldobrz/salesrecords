using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Notification raised when an entire sale is cancelled.
/// </summary>
public class SaleCancelledEvent : INotification
{
    public Guid SaleId { get; init; }
    public int SaleNumber { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
