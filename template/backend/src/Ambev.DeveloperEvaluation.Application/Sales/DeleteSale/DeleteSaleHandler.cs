using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Application.Sales.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Handler for <see cref="DeleteSaleCommand"/>.
/// Cancels the sale (logical delete), persists the change,
/// and publishes a <see cref="SaleCancelledEvent"/>.
/// </summary>
public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, DeleteSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;
    private readonly ILogger<DeleteSaleHandler> _logger;

    public DeleteSaleHandler(
        ISaleRepository saleRepository,
        IPublisher publisher,
        ILogger<DeleteSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<DeleteSaleResponse> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new DeleteSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id '{command.Id}' was not found.");

        sale.Cancel();
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        var evt = new SaleCancelledEvent
        {
            SaleId = sale.Id,
            SaleNumber = sale.SaleNumber
        };

        _logger.LogInformation(
            "SaleCancelled: SaleId={SaleId}, SaleNumber={SaleNumber}",
            evt.SaleId, evt.SaleNumber);

        await _publisher.Publish(evt, cancellationToken);

        return new DeleteSaleResponse { Success = true };
    }
}
