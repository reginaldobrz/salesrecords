using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleItemRequest, UpdateSaleItemDto>();
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<UpdateSaleItemResult, UpdateSaleItemResponse>();
        CreateMap<UpdateSaleResult, UpdateSaleResponse>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
    }
}
