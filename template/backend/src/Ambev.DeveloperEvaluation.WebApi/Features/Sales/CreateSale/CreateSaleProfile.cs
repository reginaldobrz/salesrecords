using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleProfile : Profile
{
    public CreateSaleProfile()
    {
        CreateMap<CreateSaleItemRequest, CreateSaleItemDto>();
        CreateMap<CreateSaleRequest, CreateSaleCommand>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<CreateSaleItemResult, CreateSaleItemResponse>();
        CreateMap<CreateSaleResult, CreateSaleResponse>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
    }
}
