using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StealTheCats.Common;
using StealTheCats.Entities.DataTransferObjects;
using StealTheCats.Entities.Models;
using StealTheCats.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StealTheCats.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatsController : ControllerBase
    {
        private readonly ILogger<CatsController> _logger;
        private readonly ICatService _catService;
        private readonly IMapper _mapper;
        public CatsController(ILogger<CatsController> logger, ICatService catService, IMapper mapper)
        {
            _logger = logger;
            _catService = catService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCats([FromQuery] CatParameters QueryParam)
        {
            PagedList<CatDto> cats;
            if (string.IsNullOrEmpty(QueryParam.Tag))
                cats = await _catService.GetCatsAsync(QueryParam);
            else
                cats = await _catService.GetCatsByTagAsync(QueryParam);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var cat = await _catService.GetCatById(id);
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
                _logger.LogError($"Something went wrong in Get action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
