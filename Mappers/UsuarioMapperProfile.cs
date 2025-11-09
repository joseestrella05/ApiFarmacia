using ApiFarmacia.Dto;
using ApiFarmacia.Models;
using AutoMapper;

namespace ApiFarmacia.Mappers;

public class UsuarioMapperProfile : Profile
{
    public UsuarioMapperProfile()
    {
        CreateMap<Usuario, UsuarioReadDto>();
        CreateMap<RegistroUsuarioDto, Usuario>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
    }
}