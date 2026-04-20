using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Unit tests for <see cref="DeleteSaleHandler"/>.
/// </summary>
public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;
    private readonly ILogger<DeleteSaleHandler> _logger;
    private readonly DeleteSaleHandler _handler;

    public DeleteSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        _logger = Substitute.For<ILogger<DeleteSaleHandler>>();
        _handler = new DeleteSaleHandler(_saleRepository, _publisher, _logger);
    }

    [Fact(DisplayName = "Given existing sale When Handle Then cancels sale and publishes event")]
    public async Task Given_ExistingSale_When_Handle_Then_CancelsSaleAndPublishesEvent()
    {
        // Given
        var sale = CreateSaleHandlerTestData.GenerateSaleFromCommand(
            CreateSaleHandlerTestData.GenerateValidCommand());

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);

        var command = new DeleteSaleCommand(sale.Id);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
        sale.IsCancelled.Should().BeTrue();
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(Arg.Any<SaleCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When Handle Then throws KeyNotFoundException")]
    public async Task Given_NonExistentSale_When_Handle_Then_ThrowsKeyNotFoundException()
    {
        // Given
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var command = new DeleteSaleCommand(saleId);

        // When
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{saleId}*");
    }
}
