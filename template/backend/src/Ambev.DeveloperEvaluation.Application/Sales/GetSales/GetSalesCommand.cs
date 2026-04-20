using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>Query to retrieve a paginated list of all sales.</summary>
public record GetSalesCommand(int Page, int Size) : IRequest<GetSalesResult>;
