using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Unit tests for <see cref="CreateSaleHandler"/>.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateSaleHandler> _logger;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _publisher = Substitute.For<IPublisher>();
        _logger = Substitute.For<ILogger<CreateSaleHandler>>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _publisher, _logger);
    }

    [Fact(DisplayName = "Given valid command When Handle Then creates sale and returns result")]
    public async Task Given_ValidCommand_When_Handle_Then_CreatesSaleAndReturnsResult()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var sale = CreateSaleHandlerTestData.GenerateSaleFromCommand(command);

        var expectedResult = new CreateSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            SaleDate = sale.SaleDate,
            CustomerName = sale.CustomerName,
            BranchName = sale.BranchName,
            TotalAmount = sale.TotalAmount,
            IsCancelled = false,
            Items = new List<CreateSaleItemResult>()
        };

        _mapper.Map<Sale>(command).Returns(new Sale
        {
            Id = Guid.NewGuid(),
            SaleDate = command.SaleDate,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            BranchId = command.BranchId,
            BranchName = command.BranchName
        });

        _mapper.Map<SaleItem>(Arg.Any<CreateSaleItemDto>()).Returns(callInfo =>
        {
            var dto = callInfo.Arg<CreateSaleItemDto>();
            return new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice
            };
        });

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);

        _mapper.Map<CreateSaleResult>(sale).Returns(expectedResult);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.CustomerName.Should().Be(command.CustomerName);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid command When Handle Then throws ValidationException")]
    public async Task Given_InvalidCommand_When_Handle_Then_ThrowsValidationException()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateInvalidCommand();

        // When
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
    }
}
