using AutoMapper;
using StealTheCats.Entities.DataTransferObjects;
using StealTheCats.Entities.Models;

namespace StealTheCats
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Cat, CatDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(c => c.Name).ToList()));

            CreateMap<CatImageDto, Cat>()
            .ForMember(dest => dest.CatId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Tags, opt => opt.Ignore());

            CreateMap<CatBreedDto, Tag>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Temperament))
                .ForMember(dest => dest.Created, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Cats, opt => opt.Ignore());
        }
    }
}
