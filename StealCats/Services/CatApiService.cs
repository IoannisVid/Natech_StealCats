using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using StealTheCats.Entities.DataTransferObjects;
using StealTheCats.Interfaces;

namespace StealTheCats.Services
{
    public class CatApiService : ICatApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatApiService> _logger;
        public CatApiService(HttpClient httpClient, ILogger<CatApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<List<CatImageDto>> GetCatImagesAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    "v1/images/search?size=med&format=json&has_breeds=true&limit=25");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                List<CatImageDto> catImages = JsonSerializer.Deserialize<List<CatImageDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if(catImages.IsNullOrEmpty())
                {
                    return new List<CatImageDto>();
                }
                foreach(var cat in catImages)
                {
                    cat.Image = await GetImageAsync(cat.Url);
                }
                return catImages;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error trying to fetch cats: {ex.Message}");
                return new List<CatImageDto>();
            }
        }
        private async Task<byte[]> GetImageAsync(string url)
        {
            return await _httpClient.GetByteArrayAsync(url);
        }
    }
}
