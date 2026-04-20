using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>Command to cancel (logically delete) a sale by its identifier.</summary>
public record DeleteSaleCommand(Guid Id) : IRequest<DeleteSaleResponse>;
