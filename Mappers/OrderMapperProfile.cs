using ApiFarmacia.Dto;
using ApiFarmacia.Models;
using AutoMapper;

namespace ApiFarmacia.Mappers;

public class OrderMapperProfile : Profile
{
    public OrderMapperProfile()
    {
        CreateMap<Order, OrderResponseDto>()
            .ForMember(dest => dest.Productos, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss")))
            .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => src.FechaActualizacion.ToString("yyyy-MM-ddTHH:mm:ss")));

        CreateMap<OrderItem, OrderProductDto>()
            .ForMember(dest => dest.ProductoId, opt => opt.MapFrom(src => src.ProductoId))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio));
    }
}
