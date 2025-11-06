using ApiFarmacia.Dto;
using ApiFarmacia.Models;
using AutoMapper;

namespace ApiFarmacia.Mappers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Productos, ProductoReadDto>();
        CreateMap<ProductoDto, Productos>();
    }
}