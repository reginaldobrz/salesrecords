using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleItemDto, SaleItem>();
        CreateMap<SaleItem, UpdateSaleItemResult>();
        CreateMap<Sale, UpdateSaleResult>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
    }
}
