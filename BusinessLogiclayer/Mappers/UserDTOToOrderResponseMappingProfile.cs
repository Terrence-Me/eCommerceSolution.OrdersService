﻿using AutoMapper;
using BusinessLogiclayer.DTO;

namespace BusinessLogiclayer.Mappers;
public class UserDTOToOrderResponseMappingProfile : Profile
{
    public UserDTOToOrderResponseMappingProfile()
    {
        CreateMap<UserDTO, OrderResponse>()
            .ForMember(dest => dest.UserPersonName, opt => opt.MapFrom(src => src.PersonName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

    }
}
