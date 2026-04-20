using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>Query to retrieve a sale by its unique identifier.</summary>
public record GetSaleCommand(Guid Id) : IRequest<GetSaleResult>;
