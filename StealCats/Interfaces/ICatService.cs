using StealTheCats.Entities.DataTransferObjects;
using StealTheCats.Entities.Models;
using StealTheCats.Common;

namespace StealTheCats.Interfaces
{
    public interface ICatService
    {
        Task<Cat?> GetCatById(string id);
        Task<PagedList<CatDto>> GetCatsAsync(CatParameters QueryParam);
        Task<PagedList<CatDto>> GetCatsByTagAsync(CatParameters QueryParam);
    }
}
