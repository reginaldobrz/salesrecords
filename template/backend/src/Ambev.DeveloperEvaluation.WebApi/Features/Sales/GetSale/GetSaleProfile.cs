using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

public class GetSaleProfile : Profile
{
    public GetSaleProfile()
    {
        CreateMap<GetSaleItemResult, GetSaleItemResponse>();
        CreateMap<GetSaleResult, GetSaleResponse>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
    }
}
