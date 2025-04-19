using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CatService> _logger;
        private readonly CacheInvalidationToken _token;

        public CatService(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache memoryCache, CacheInvalidationToken token, ILogger<CatService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _memoryCache = memoryCache;
            _token = token;
            _logger = logger;
        }
        public async Task<Cat?> GetCatByIdWithTagAsync(string id) => await _unitOfWork.GetRepository<Cat>()
            .GetByCondition(x => x.CatId.Equals(id)).Include(x => x.Tags).FirstOrDefaultAsync();
        public async Task<Cat?> GetCatByIdAsync(string id) => await _unitOfWork.GetRepository<Cat>()
            .GetByCondition(x => x.CatId.Equals(id)).FirstOrDefaultAsync();

        public async Task<Dictionary<string, Cat>> GetCatDictAsync(bool track = false)
        {
            var catList = await _unitOfWork.GetRepository<Cat>().GetAll(track).Include(x => x.Tags).ToListAsync();
            return catList.ToDictionary(x => x.CatId, x => x);
        }
        public async Task<PagedList<CatDto>> GetCatsAsync(CatParameters QueryParam)
        {
            var query = _unitOfWork.GetRepository<Cat>().GetAll().Include(x => x.Tags);
            var pagedList = await PagedList<Cat>.ToPagedList(query, QueryParam.PageNumber, QueryParam.PageSize);
            var dtoList = _mapper.Map<List<CatDto>>(pagedList);

            return new PagedList<CatDto>(dtoList, pagedList.TotalCount, pagedList.CurrentPage, pagedList.PageSize);
        }
        public async Task<PagedList<CatDto>> GetCatsByTagAsync(CatParameters QueryParam)
        {
            var query = _unitOfWork.GetRepository<Cat>().GetByCondition(x => x.Tags.Any(x => x.Name.ToLower().Equals(QueryParam.Tag.ToLower())));
            var pagedList = await PagedList<Cat>.ToPagedList(query, QueryParam.PageNumber, QueryParam.PageSize);
            var dtoList = _mapper.Map<List<CatDto>>(pagedList.ToList());

            return new PagedList<CatDto>(dtoList, pagedList.TotalCount, pagedList.CurrentPage, pagedList.PageSize);
        }

        public async Task CreateCatsAsync(List<CatImageDto> CatImages)
        {
            if (!_memoryCache.TryGetValue("CatsDict", out Dictionary<string, Cat> catsDict))
                catsDict = await GetCatDictAsync(true);
            if (!_memoryCache.TryGetValue("Tags", out List<Tag> existingTags))
                existingTags = await _unitOfWork.GetRepository<Tag>().GetAll(true).ToListAsync();

            foreach (var catImage in CatImages)
            {
                bool update = false;
                if (catsDict.TryGetValue(catImage.Id, out Cat cat))
                {
                    update = true;
                    _mapper.Map(catImage, cat);
                }
                else
                    cat = _mapper.Map<Cat>(catImage);

                if (!ValidateCat(cat))
                    continue;

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
                            tag = new Tag { Name = temp.Trim(), Created = DateTime.UtcNow };
                            _unitOfWork.GetRepository<Tag>().Create(tag);
                            existingTags.Add(tag);
                        }

                        if (!cat.Tags.Any(t => t.Name.Equals(temp.Trim())))
                            cat.Tags.Add(tag);
                    }
                    var removeTags = cat.Tags.Where(x => Temperaments.Contains(x.Name)).ToList();
                    foreach (var remTag in removeTags)
                        cat.Tags.Remove(remTag);
                }
                if (update)
                    _unitOfWork.GetRepository<Cat>().Update(cat);
                else
                    _unitOfWork.GetRepository<Cat>().Create(cat);
            }
            await _unitOfWork.SaveAsync();
            _token.Invalidate();
        }

        private bool ValidateCat(Cat cat)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(cat);
            bool isValidCat = Validator.TryValidateObject(cat, validationContext, validationResults, true);
            if (!isValidCat)
            {
                _logger.LogError($"Incorrect structure for CatId:{cat.CatId}:");
                validationResults.ForEach(err => _logger.LogError(err.ErrorMessage));
                return false;
            }
            return true;
        }
    }
}
