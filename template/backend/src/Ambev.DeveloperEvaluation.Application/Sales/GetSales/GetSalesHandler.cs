using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>Handler for <see cref="GetSalesCommand"/>. Returns a paginated list of sales.</summary>
public class GetSalesHandler : IRequestHandler<GetSalesCommand, GetSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<GetSalesResult> Handle(GetSalesCommand command, CancellationToken cancellationToken)
    {
        var validator = new GetSalesValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var (items, totalCount) = await _saleRepository.GetAllAsync(command.Page, command.Size, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)command.Size);

        return new GetSalesResult
        {
            Items = _mapper.Map<IEnumerable<GetSalesItemResult>>(items),
            TotalItems = totalCount,
            CurrentPage = command.Page,
            TotalPages = totalPages
        };
    }
}
