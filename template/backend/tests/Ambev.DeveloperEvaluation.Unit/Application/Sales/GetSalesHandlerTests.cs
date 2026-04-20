using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
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
/// Unit tests for <see cref="GetSalesHandler"/>.
/// </summary>
public class GetSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSalesHandler _handler;

    public GetSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSalesHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid page and size When Handle Then returns paginated result")]
    public async Task Given_ValidCommand_When_Handle_Then_ReturnsPaginatedResult()
    {
        // Given
        var command = new GetSalesCommand(Page: 2, Size: 10);

        var sales = new List<Sale>
        {
            CreateSaleHandlerTestData.GenerateSaleFromCommand(CreateSaleHandlerTestData.GenerateValidCommand()),
            CreateSaleHandlerTestData.GenerateSaleFromCommand(CreateSaleHandlerTestData.GenerateValidCommand())
        };
        const int totalCount = 25;

        _saleRepository.GetAllAsync(command.Page, command.Size, Arg.Any<CancellationToken>())
            .Returns((sales.AsEnumerable(), totalCount));

        var mappedItems = sales.Select(s => new GetSalesItemResult { CustomerName = s.CustomerName }).ToList();
        _mapper.Map<IEnumerable<GetSalesItemResult>>(Arg.Any<IEnumerable<Sale>>())
            .Returns(mappedItems);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(totalCount);
        result.CurrentPage.Should().Be(command.Page);
        result.TotalPages.Should().Be(3); // Math.Ceiling(25 / 10.0)
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Given page 0 When Handle Then throws ValidationException")]
    public async Task Given_PageZero_When_Handle_Then_ThrowsValidationException()
    {
        // Given
        var command = new GetSalesCommand(Page: 0, Size: 10);

        // When
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Given size 0 When Handle Then throws ValidationException")]
    public async Task Given_SizeZero_When_Handle_Then_ThrowsValidationException()
    {
        // Given
        var command = new GetSalesCommand(Page: 1, Size: 0);

        // When
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Given size above 100 When Handle Then throws ValidationException")]
    public async Task Given_SizeAbove100_When_Handle_Then_ThrowsValidationException()
    {
        // Given
        var command = new GetSalesCommand(Page: 1, Size: 101);

        // When
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Given no sales exist When Handle Then returns empty items list")]
    public async Task Given_NoSales_When_Handle_Then_ReturnsEmptyList()
    {
        // Given
        var command = new GetSalesCommand(Page: 1, Size: 10);

        _saleRepository.GetAllAsync(command.Page, command.Size, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Sale>(), 0));

        _mapper.Map<IEnumerable<GetSalesItemResult>>(Arg.Any<IEnumerable<Sale>>())
            .Returns(Enumerable.Empty<GetSalesItemResult>());

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.TotalItems.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}
