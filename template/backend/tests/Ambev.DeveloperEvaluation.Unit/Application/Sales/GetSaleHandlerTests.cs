using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Unit tests for <see cref="GetSaleHandler"/>.
/// </summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing sale When Handle Then returns mapped result")]
    public async Task Given_ExistingSale_When_Handle_Then_ReturnsMappedResult()
    {
        // Given
        var saleId = Guid.NewGuid();
        var command = new GetSaleCommand(saleId);

        var sale = CreateSaleHandlerTestData.GenerateSaleFromCommand(
            CreateSaleHandlerTestData.GenerateValidCommand());
        sale.Id = saleId;

        var expectedResult = new GetSaleResult
        {
            Id = saleId,
            CustomerName = sale.CustomerName,
            SaleNumber = sale.SaleNumber
        };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<GetSaleResult>(sale)
            .Returns(expectedResult);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Id.Should().Be(saleId);
    }

    [Fact(DisplayName = "Given Guid.Empty id When Handle Then throws ValidationException")]
    public async Task Given_EmptyId_When_Handle_Then_ThrowsValidationException()
    {
        // Given
        var command = new GetSaleCommand(Guid.Empty);

        // When
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Given valid id but sale not found When Handle Then throws KeyNotFoundException")]
    public async Task Given_SaleNotFound_When_Handle_Then_ThrowsKeyNotFoundException()
    {
        // Given
        var saleId = Guid.NewGuid();
        var command = new GetSaleCommand(saleId);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // When
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{saleId}*");
    }
}
