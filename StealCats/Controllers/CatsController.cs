using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using StealTheCats.Common;
using StealTheCats.Entities.DataTransferObjects;
using StealTheCats.Entities.Models;
using StealTheCats.Interfaces;

namespace StealTheCats.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatsController : ControllerBase
    {
        private readonly ILogger<CatsController> _logger;
        private readonly ICatService _catService;
        private readonly ICatApiService _catApiService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheInvalidationToken _token;

        public CatsController(ILogger<CatsController> logger, ICatService catService, IMapper mapper, ICatApiService catApiService, IMemoryCache memoryCache, CacheInvalidationToken token)
        {
            _logger = logger;
            _catService = catService;
            _mapper = mapper;
            _catApiService = catApiService;
            _memoryCache = memoryCache;
            _token = token;
        }

        [HttpGet]
        public async Task<IActionResult> GetCats([FromQuery] CatParameters QueryParam)
        {
            try
            {
                string cacheKey = QueryParam.GetKeyString();
                if (!_memoryCache.TryGetValue(cacheKey, out PagedList<CatDto> cats))
                {
                    if (string.IsNullOrEmpty(QueryParam.Tag))
                        cats = await _catService.GetCatsAsync(QueryParam);
                    else
                        cats = await _catService.GetCatsByTagAsync(QueryParam);
                }
                var options = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = new TimeSpan(0, 5, 0)
                };
                options.AddExpirationToken(new CancellationChangeToken(_token.Token));
                _memoryCache.Set(cacheKey, cats, options);
                var metadata = new
                {
                    cats.TotalCount,
                    cats.PageSize,
                    cats.CurrentPage,
                    cats.TotalPages,
                    cats.HasNext,
                    cats.HasPrevious
                };
                Response.Headers.Add("Pagination", JsonSerializer.Serialize(metadata));
                if (cats.Count == 0)
                    return NotFound();
                return Ok(cats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in Get action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var cacheKey = $"Cat:{id}";
                var cat = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SlidingExpiration = new TimeSpan(0, 10, 0);
                    entry.AddExpirationToken(new CancellationChangeToken(_token.Token));
                    return await _catService.GetCatByIdAsync(id);
                });
                if (cat == null)
                {
                    _logger.LogError($"Cat with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    var catResult = _mapper.Map<CatDto>(cat);
                    return Ok(catResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in Get with id action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("fetch")]
        public async Task<IActionResult> FetchCats()
        {
            try
            {
                var CatImages = await _catApiService.GetCatImagesAsync();
                if (CatImages.IsNullOrEmpty())
                {
                    _logger.LogError($"Failed to fetch any cats");
                    return StatusCode(500, "Internal server error");
                }
                await _catService.CreateCatsAsync(CatImages.Distinct().ToList());
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in fetching action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
