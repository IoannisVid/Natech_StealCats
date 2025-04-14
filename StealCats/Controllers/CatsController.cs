using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CatsController(ILogger<CatsController> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCats([FromQuery] CatParameters parameters)
        {
            return NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var cat = await _unitOfWork.GetRepository<Cat>().GetByCondition(x => x.CatId.Equals(id)).Include(x => x.Tags).FirstOrDefaultAsync();
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

        // POST api/<StealTheCatsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<StealTheCatsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<StealTheCatsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
