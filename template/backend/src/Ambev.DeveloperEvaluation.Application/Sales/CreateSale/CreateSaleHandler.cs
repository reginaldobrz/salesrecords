using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Application.Sales.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handler for <see cref="CreateSaleCommand"/>.
/// Validates the command, builds the Sale aggregate with discount calculations,
/// persists it, and publishes a <see cref="SaleCreatedEvent"/>.
/// </summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateSaleHandler> _logger;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        IPublisher publisher,
        ILogger<CreateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Build aggregate
        var sale = _mapper.Map<Sale>(command);
        sale.Id = Guid.NewGuid();

        foreach (var itemDto in command.Items)
        {
            var item = _mapper.Map<SaleItem>(itemDto);
            item.Id = Guid.NewGuid();
            sale.AddItem(item);
        }

        sale.CalculateTotalAmount();

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        // Publish domain event (logged only — no broker required)
        var evt = new SaleCreatedEvent
        {
            SaleId = created.Id,
            SaleNumber = created.SaleNumber,
            CustomerName = created.CustomerName,
            TotalAmount = created.TotalAmount
        };

        _logger.LogInformation(
            "SaleCreated: SaleId={SaleId}, SaleNumber={SaleNumber}, Customer={CustomerName}, Total={TotalAmount}",
            evt.SaleId, evt.SaleNumber, evt.CustomerName, evt.TotalAmount);

        await _publisher.Publish(evt, cancellationToken);

        return _mapper.Map<CreateSaleResult>(created);
    }
}
