using AutoMapper;
using BusinessLogiclayer.DTO;

namespace BusinessLogiclayer.Mappers;
public class ProductDTOToOrderItemResponseMappingProfile : Profile
{
    public ProductDTOToOrderItemResponseMappingProfile()
    {
        CreateMap<ProductDTO, OrderItemResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
    }
}
