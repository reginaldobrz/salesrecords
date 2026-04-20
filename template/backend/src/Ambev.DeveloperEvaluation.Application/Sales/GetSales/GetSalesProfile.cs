using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

public class GetSalesProfile : Profile
{
    public GetSalesProfile()
    {
        CreateMap<Sale, GetSalesItemResult>();
    }
}
