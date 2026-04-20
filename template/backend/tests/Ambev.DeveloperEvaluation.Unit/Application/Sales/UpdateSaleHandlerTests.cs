using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Unit tests for <see cref="UpdateSaleHandler"/>.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ILogger<UpdateSaleHandler> _logger;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _publisher = Substitute.For<IPublisher>();
        _logger = Substitute.For<ILogger<UpdateSaleHandler>>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _publisher, _logger);
    }

    [Fact(DisplayName = "Given sale not found When Handle Then throws KeyNotFoundException")]
    public async Task Given_SaleNotFound_When_Handle_Then_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch",
            Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 2,
                    UnitPrice = 50m
                }
            }
        };

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // When
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.Id}*");
    }

    [Fact(DisplayName = "Given cancelled sale When Handle Then throws InvalidOperationException")]
    public async Task Given_CancelledSale_When_Handle_Then_ThrowsInvalidOperationException()
    {
        // Given
        var existingSale = CreateSaleHandlerTestData.GenerateSaleFromCommand(
            CreateSaleHandlerTestData.GenerateValidCommand());
        existingSale.Cancel();

        var command = new UpdateSaleCommand
        {
            Id = existingSale.Id,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch",
            Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 2,
                    UnitPrice = 50m
                }
            }
        };

        _saleRepository.GetByIdAsync(existingSale.Id, Arg.Any<CancellationToken>())
            .Returns(existingSale);

        // When
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cancelled*");
    }

    [Fact(DisplayName = "Given valid command with active sale When Handle Then updates sale and publishes SaleModifiedEvent")]
    public async Task Given_ValidCommand_When_Handle_Then_UpdatesSaleAndPublishesSaleModifiedEvent()
    {
        // Given
        var existingSale = CreateSaleHandlerTestData.GenerateSaleFromCommand(
            CreateSaleHandlerTestData.GenerateValidCommand());

        var command = new UpdateSaleCommand
        {
            Id = existingSale.Id,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Updated Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Updated Branch",
            Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Updated Product",
                    Quantity = 5,
                    UnitPrice = 20m
                }
            }
        };

        _saleRepository.GetByIdAsync(existingSale.Id, Arg.Any<CancellationToken>())
            .Returns(existingSale);

        _mapper.Map<SaleItem>(Arg.Any<UpdateSaleItemDto>())
            .Returns(callInfo =>
            {
                var dto = callInfo.Arg<UpdateSaleItemDto>();
                return new SaleItem
                {
                    ProductId = dto.ProductId,
                    ProductName = dto.ProductName,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice
                };
            });

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        var expectedResult = new UpdateSaleResult
        {
            Id = existingSale.Id,
            CustomerName = command.CustomerName
        };
        _mapper.Map<UpdateSaleResult>(Arg.Any<Sale>())
            .Returns(expectedResult);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.CustomerName.Should().Be(command.CustomerName);

        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(Arg.Any<Ambev.DeveloperEvaluation.Application.Sales.Events.SaleModifiedEvent>(), Arg.Any<CancellationToken>());
    }
}
