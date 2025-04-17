using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StealTheCats.Common;
using StealTheCats.Entities.DataTransferObjects;
using StealTheCats.Entities.Models;
using StealTheCats.Interfaces;

namespace StealTheCats.Services
{
    public class CatService : ICatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CatService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Cat?> GetCatByIdWithTagAsync(string id) => await _unitOfWork.GetRepository<Cat>()
            .GetByCondition(x => x.CatId.Equals(id)).Include(x => x.Tags).FirstOrDefaultAsync();
        public async Task<Cat?> GetCatByIdAsync(string id) => await _unitOfWork.GetRepository<Cat>()
            .GetByCondition(x => x.CatId.Equals(id)).FirstOrDefaultAsync();

        public async Task<PagedList<CatDto>> GetCatsAsync(CatParameters QueryParam)
        {
            var query = _unitOfWork.GetRepository<Cat>().GetAll().Include(x => x.Tags);
            var pagedList = await PagedList<Cat>.ToPagedList(query, QueryParam.PageNumber, QueryParam.PageSize);
            var dtoList = _mapper.Map<List<CatDto>>(pagedList);

            return new PagedList<CatDto>(dtoList, pagedList.TotalCount, pagedList.CurrentPage, pagedList.PageSize);
        }

        public async Task<PagedList<CatDto>> GetCatsByTagAsync(CatParameters QueryParam)
        {
            var query = _unitOfWork.GetRepository<Tag>().GetByCondition(x => x.Name.Equals(QueryParam.Tag))
                .Include(x => x.Cats).SelectMany(x=>x.Cats);//.ThenInclude(x => x.Tags);
            var pagedList = await PagedList<Cat>.ToPagedList(query, QueryParam.PageNumber, QueryParam.PageSize);

            var dtoList = _mapper.Map<List<CatDto>>(pagedList.ToList());

            return new PagedList<CatDto>(dtoList, pagedList.TotalCount, pagedList.CurrentPage, pagedList.PageSize);
        }

        public async Task CreateCatsAsync(List<CatImageDto> CatImages)
        {
            List<Tag> existingTags = await _unitOfWork.GetRepository<Tag>().GetAll().ToListAsync();
            foreach (var catImage in CatImages)
            {
                if (await GetCatByIdAsync(catImage.Id) != null)
                    continue;

                var cat = _mapper.Map<Cat>(catImage);
                foreach (var catBreed in catImage.Breeds)
                {
                    var Temperaments = catBreed.Temperament.Split(',');
                    foreach (var temp in Temperaments)
                    {
                        Tag tag;
                        var existTag = existingTags.FirstOrDefault(x => x.Name.Equals(temp.Trim()));
                        if (existTag != null)
                            tag = existTag;
                        else
                        {
                            tag = new Tag { Name = temp.Trim() };
                            _unitOfWork.GetRepository<Tag>().Create(tag);
                            existingTags.Add(tag);
                        }

                        if (!cat.Tags.Any(t => t.Name.Equals(temp.Trim())))
                            cat.Tags.Add(tag);
                    }
                }
                _unitOfWork.GetRepository<Cat>().Create(cat);
            }
            await _unitOfWork.SaveAsync();
        }
    }
}
