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
        public async Task<Cat?> GetCatById(string id) => await _unitOfWork.GetRepository<Cat>()
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

    }
}
