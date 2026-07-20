using AutoMapper;
using CountryExplorer.Application.Dtos.Auth;
using CountryExplorer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Application.Mappings;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, AuthResponseDto>();
    }
}
