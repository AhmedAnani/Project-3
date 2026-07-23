using AutoMapper;
using CountryExplorer.Application.DTOs.Attractions;
using CountryExplorer.Application.DTOs.Countries;
using CountryExplorer.Infrastructure.ExternalServices.Models;

namespace CountryExplorer.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RestCountryResponse, CountryDto>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Codes.Alpha2))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Names.Common))
            .ForMember(dest => dest.OfficialName, opt => opt.MapFrom(src => src.Names.Official))
            .ForMember(dest => dest.Capital, opt => opt.MapFrom(src =>
                src.Capitals.Any() ? src.Capitals.First().Name : string.Empty))
            .ForMember(dest => dest.FlagUrl, opt => opt.MapFrom(src => src.Flag.UrlPng))
            .ForMember(dest => dest.Currencies, opt => opt.MapFrom(src =>
                src.Currencies.Select(c => c.Name).ToList()))
            .ForMember(dest => dest.Languages, opt => opt.MapFrom(src =>
                src.Languages.Select(l => l.Name).ToList()))
            .ForMember(dest => dest.CapitalLatitude, opt => opt.MapFrom(src =>
                src.Capitals.Any() ? src.Capitals.First().Coordinates.Lat : (double?)null))
            .ForMember(dest => dest.CapitalLongitude, opt => opt.MapFrom(src =>
                src.Capitals.Any() ? src.Capitals.First().Coordinates.Lng : (double?)null));

        CreateMap<OpenTripMapPlaceResponse, TouristAttractionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Xid))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                src.Kinds.Split(',',StringSplitOptions.None).FirstOrDefault() ?? string.Empty))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Point.Lat))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Point.Lon))
            .ForMember(dest => dest.DistanceFromCenterMeters, opt => opt.MapFrom(src => src.Dist));

        CreateMap<OpenTripMapPlaceDetailsResponse, TouristAttractionDetailsDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Xid))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src =>
                src.Kinds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Point.Lat))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Point.Lon))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                src.Preview != null ? src.Preview.Source : string.Empty)) 
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                src.WikipediaExtracts != null ? src.WikipediaExtracts.Text : string.Empty));
    }
}