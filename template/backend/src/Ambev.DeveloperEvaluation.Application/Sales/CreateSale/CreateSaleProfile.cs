using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// AutoMapper profile for the CreateSale operation.
/// </summary>
public class CreateSaleProfile : Profile
{
    public CreateSaleProfile()
    {
        CreateMap<CreateSaleItemDto, SaleItem>();
        CreateMap<CreateSaleCommand, Sale>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<SaleItem, CreateSaleItemResult>();
        CreateMap<Sale, CreateSaleResult>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
    }
}
