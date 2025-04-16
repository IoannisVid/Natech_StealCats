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
        public async Task<Cat?> GetCatByIdAsync(string id) => await _unitOfWork.GetRepository<Cat>()
            .GetByCondition(x => x.CatId.Equals(id)).Include(x => x.Tags).FirstOrDefaultAsync();

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
                .Include(x => x.Cats).ThenInclude(x => x.Tags);
            var pagedList = await PagedList<Tag>.ToPagedList(query, QueryParam.PageNumber, QueryParam.PageSize);
            var dtoList = _mapper.Map<List<CatDto>>(pagedList);

            return new PagedList<CatDto>(dtoList, pagedList.TotalCount, pagedList.CurrentPage, pagedList.PageSize);
        }

        public async Task CreateCatsAsync(List<CatImageDto> CatImages)
        {
            foreach (var catImage in CatImages)
            {
                if (await GetCatByIdAsync(catImage.Id) != null)
                    continue;

                var cat = _mapper.Map<Cat>(catImage);
                foreach (var catBreed in catImage.Breeds)
                {
                    var tagName = catBreed.Temperament;

                    var tag = await _unitOfWork.GetRepository<Tag>().GetByCondition(x => x.Name.Equals(catBreed.Temperament)).FirstOrDefaultAsync();
                    if (tag == null)
                    {
                        tag = _mapper.Map<Tag>(catBreed);
                        _unitOfWork.GetRepository<Tag>().Create(tag);
                    }

                    if (!cat.Tags.Any(t => t.Name.Equals(tagName)))
                    {
                        cat.Tags.Add(tag);
                    }
                }
                _unitOfWork.GetRepository<Cat>().Create(cat);
            }
            await _unitOfWork.SaveAsync();
        }
    }
}
