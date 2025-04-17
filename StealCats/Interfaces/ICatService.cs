using StealTheCats.Entities.DataTransferObjects;
using StealTheCats.Entities.Models;
using StealTheCats.Common;

namespace StealTheCats.Interfaces
{
    public interface ICatService
    {
        Task<Cat?> GetCatByIdAsync(string id);
        Task<Cat?> GetCatByIdWithTagAsync(string id);
        Task<PagedList<CatDto>> GetCatsAsync(CatParameters QueryParam);
        Task<PagedList<CatDto>> GetCatsByTagAsync(CatParameters QueryParam);
        Task CreateCatsAsync(List<CatImageDto> CatImages);
    }
}
