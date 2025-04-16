using StealTheCats.Entities.DataTransferObjects;

namespace StealTheCats.Interfaces
{
    public interface ICatApiService
    {
        Task<List<CatImageDto>> GetCatImagesAsync();
    }
}
