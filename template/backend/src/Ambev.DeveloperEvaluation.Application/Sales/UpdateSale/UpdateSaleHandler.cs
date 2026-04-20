using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Application.Sales.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for <see cref="UpdateSaleCommand"/>.
/// Replaces the sale's header and items, recalculates discounts and totals,
/// persists the changes, and publishes a <see cref="SaleModifiedEvent"/>.
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ILogger<UpdateSaleHandler> _logger;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        IPublisher publisher,
        ILogger<UpdateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id '{command.Id}' was not found.");

        if (sale.IsCancelled)
            throw new InvalidOperationException($"Sale '{command.Id}' is cancelled and cannot be modified.");

        // Update header fields
        sale.SaleDate = command.SaleDate;
        sale.CustomerId = command.CustomerId;
        sale.CustomerName = command.CustomerName;
        sale.BranchId = command.BranchId;
        sale.BranchName = command.BranchName;
        sale.UpdatedAt = DateTime.UtcNow;

        // Replace items: clear and rebuild using the backing field via reflection-safe pattern
        var newItems = command.Items.Select(dto =>
        {
            var item = _mapper.Map<SaleItem>(dto);
            item.Id = Guid.NewGuid();
            item.SaleId = sale.Id;
            item.CalculateDiscount();
            return item;
        }).ToList();

        // Clear the private _items list via the Items property
        // Since Sale exposes IReadOnlyCollection, we use the internal method approach:
        sale.ReplaceItems(newItems);
        sale.CalculateTotalAmount();

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        var evt = new SaleModifiedEvent
        {
            SaleId = updated.Id,
            SaleNumber = updated.SaleNumber
        };

        _logger.LogInformation(
            "SaleModified: SaleId={SaleId}, SaleNumber={SaleNumber}",
            evt.SaleId, evt.SaleNumber);

        await _publisher.Publish(evt, cancellationToken);

        return _mapper.Map<UpdateSaleResult>(updated);
    }
}
