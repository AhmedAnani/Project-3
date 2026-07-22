using AutoMapper;
using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Domain.Entities;


namespace CountryExplorer.Application.Mappings;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, UserProfileDto>()
           .ForMember(dest => dest.Role,
               opt => opt.MapFrom(src => src.Role.ToString()))
           .ReverseMap();

        
        CreateMap<RefreshToken, RefreshTokenDto>()
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(src => src.IsActive))
            .ReverseMap();

        
        CreateMap<User, AuthResponseDto>()
            .ForMember(dest => dest.User,
                opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.AccessToken,
                opt => opt.Ignore())  
            .ForMember(dest => dest.RefreshToken,
                opt => opt.Ignore())  
            .ForMember(dest => dest.AccessTokenExpiresAt,
                opt => opt.Ignore()); 
    }
}
