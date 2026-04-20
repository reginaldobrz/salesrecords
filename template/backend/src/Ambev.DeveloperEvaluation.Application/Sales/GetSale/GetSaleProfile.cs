using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleProfile : Profile
{
    public GetSaleProfile()
    {
        CreateMap<SaleItem, GetSaleItemResult>();
        CreateMap<Sale, GetSaleResult>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
    }
}
