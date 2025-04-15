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
        }
    }
}
