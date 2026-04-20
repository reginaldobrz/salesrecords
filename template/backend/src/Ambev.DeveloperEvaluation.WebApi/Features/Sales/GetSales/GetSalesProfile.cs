using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;

public class GetSalesProfile : Profile
{
    public GetSalesProfile()
    {
        CreateMap<GetSalesItemResult, GetSalesItemResponse>();
        CreateMap<GetSalesResult, GetSalesResponse>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Items));
    }
}
