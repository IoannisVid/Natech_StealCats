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
                .ForMember(dest => dest.Temperament, opt => opt.MapFrom(src => string.Join(", ", src.Tags.Select(c => c.Name))));
            CreateMap<Tag, CatDto>()
                .ForMember(dest => dest.Temperament, opt => opt.MapFrom(src => src.Name));
        }
    }
}
