using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using ProductGrpc.Protos;

namespace ProductGrpc.Mapper
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Models.Product, ProductModel>()
                .ForMember(dest => dest.CreatedDate,
                opt => opt.MapFrom(src => Timestamp.FromDateTime(src.CreatedDate)));

            CreateMap<ProductModel, Models.Product>()
                .ForMember(dest => dest.CreatedDate,
                opt => opt.MapFrom(src => src.CreatedDate.ToDateTime()));
        }
    }
}
