using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using StealTheCats.Common;
using StealTheCats.Controllers;
using StealTheCats.Entities.DataTransferObjects;
using StealTheCats.Entities.Models;
using StealTheCats.Interfaces;

namespace StealCats.test.Controllers
{
    public class CatsControllerTests
    {
        private readonly Mock<ILogger<CatsController>> _mockLogger;
        private readonly Mock<ICatService> _mockCatService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICatApiService> _mockCatApiService;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly CacheInvalidationToken _token;

        public CatsControllerTests()
        {
            _mockLogger = new Mock<ILogger<CatsController>>();
            _mockCatService = new Mock<ICatService>();
            _mockMapper = new Mock<IMapper>();
            _mockCatApiService = new Mock<ICatApiService>();
            _mockMemoryCache = new Mock<IMemoryCache>();
            _token = new CacheInvalidationToken();
        }

        private CatsController CreateController()
        {
            var controller = new CatsController(
                _mockLogger.Object,
                _mockCatService.Object,
                _mockMapper.Object,
                _mockCatApiService.Object,
                _mockMemoryCache.Object,
                _token);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            return controller;
        }

        private Mock<ICacheEntry> CreateMockCacheEntry()
        {
            var mockCacheEntry = new Mock<ICacheEntry>();
            mockCacheEntry.SetupAllProperties();
            var tokenList = new List<IChangeToken>();
            mockCacheEntry.SetupGet(e => e.ExpirationTokens).Returns(tokenList);
            mockCacheEntry.Setup(e => e.Dispose());
            return mockCacheEntry;
        }

        [Fact]
        public async Task GetCats_ReturnsOk_WhenCacheMissAndCatsFound()
        {
            var queryParam = new CatParameters();
            var cacheKey = queryParam.GetKeyString();
            var catList = new PagedList<CatDto>([new CatDto { CatId = "abcd", Height = 100, Width = 100, CreatedAt = DateTime.UtcNow }], 1, 1, 10);

            object dummy;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(cacheKey, out dummy)).Returns(false);

            var mockCacheEntry = CreateMockCacheEntry();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                            .Returns(mockCacheEntry.Object);

            _mockCatService.Setup(cs => cs.GetCatsAsync(queryParam)).ReturnsAsync(catList);


            var controller = CreateController();

            var result = await controller.GetCats(queryParam);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCats = Assert.IsAssignableFrom<IEnumerable<CatDto>>(okResult.Value);
            Assert.Single(returnedCats);
        }

        [Fact]
        public async Task GetCats_ReturnsOk_WhenCacheHit()
        {
            var queryParam = new CatParameters();
            var cacheKey = queryParam.GetKeyString();
            var catList = new PagedList<CatDto>([new CatDto { CatId = "abcd", Height = 100, Width = 100, CreatedAt = DateTime.UtcNow }], 1, 1, 10);

            object dummy = catList;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(cacheKey, out dummy)).Returns(true);

            var controller = CreateController();

            var result = await controller.GetCats(queryParam);


            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCats = Assert.IsAssignableFrom<IEnumerable<CatDto>>(okResult.Value);
            Assert.Single(returnedCats);
        }

        [Fact]
        public async Task GetCats_WithTag_ReturnsOk_WhenCacheHit()
        {
            var queryParam = new CatParameters() { Tag = "Active" };
            var cacheKey = queryParam.GetKeyString();
            var catList = new PagedList<CatDto>(new List<CatDto>
                { new CatDto{ CatId = "abcd", Height = 100, Width = 100, CreatedAt = DateTime.UtcNow }}, 1, 1, 10);

            object dummy = catList;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(cacheKey, out dummy)).Returns(true);

            var mockCacheEntry = CreateMockCacheEntry();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                            .Returns(mockCacheEntry.Object);

            var controller = CreateController();

            var result = await controller.GetCats(queryParam);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCats = Assert.IsAssignableFrom<IEnumerable<CatDto>>(okResult.Value);
            Assert.Single(returnedCats);
        }

        [Fact]
        public async Task GetCats_ReturnsNotFound_WhenNoCatsFound()
        {
            var queryParam = new CatParameters();
            var cacheKey = queryParam.GetKeyString();
            var emptyList = new PagedList<CatDto>(new List<CatDto>(), 0, 1, 10);

            object dummy;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(cacheKey, out dummy)).Returns(false);

            var mockCacheEntry = CreateMockCacheEntry();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            _mockCatService.Setup(cs => cs.GetCatsAsync(queryParam)).ReturnsAsync(emptyList);

            var controller = CreateController();

            var result = await controller.GetCats(queryParam);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCats_ReturnsInternalServerError_OnException()
        {
            var queryParam = new CatParameters();
            var cacheKey = queryParam.GetKeyString();

            object dummy;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(cacheKey, out dummy)).Returns(false);

            _mockCatService.Setup(cs => cs.GetCatsAsync(queryParam))
                           .ThrowsAsync(new Exception("Something went wrong"));

            var controller = CreateController();

            var result = await controller.GetCats(queryParam);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal server error", objectResult.Value);
        }

        [Fact]
        public async Task Get_ReturnsOkResult_WhenCacheHit()
        {
            var id = "abcd";
            var cacheKey = $"Cat:{id}";
            var cat = new Cat { CatId = "abcd", Height = 100, Width = 100, Created = DateTime.UtcNow };
            var catDto = new CatDto { CatId = "abcd", Height = 100, Width = 100, CreatedAt = DateTime.UtcNow };

            object dummy = cat;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(cacheKey, out dummy)).Returns(true);
            var mockCacheEntry = CreateMockCacheEntry();

            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            _mockMapper.Setup(m => m.Map<CatDto>(cat)).Returns(catDto);

            var controller = CreateController();

            var result = await controller.Get(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCat = Assert.IsType<CatDto>(okResult.Value);
            Assert.Equal(id, returnedCat.CatId);
        }

        [Fact]
        public async Task Get_ReturnsOkResult_WhenCacheMissCatFound()
        {
            var id = "abcd";
            var cacheKey = $"Cat:{id}";
            var cat = new Cat { CatId = "abcd", Height = 100, Width = 100, Created = DateTime.UtcNow };
            var catDto = new CatDto { CatId = "abcd", Height = 100, Width = 100, CreatedAt = DateTime.UtcNow };

            object dummy = cat;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(cacheKey, out dummy)).Returns(false);
            var mockCacheEntry = CreateMockCacheEntry();

            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            _mockCatService.Setup(cs => cs.GetCatByIdAsync(id)).ReturnsAsync(cat);

            _mockMapper.Setup(m => m.Map<CatDto>(cat)).Returns(catDto);

            var controller = CreateController();

            var result = await controller.Get(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCat = Assert.IsType<CatDto>(okResult.Value);
            Assert.Equal(id, returnedCat.CatId);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenCatIsNull()
        {
            var id = "notFound";
            var cacheKey = $"Cat:{id}";

            object dummy;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(cacheKey, out dummy)).Returns(false);

            var controller = CreateController();

            var result = await controller.Get(id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Get_ReturnsInternalServerError_OnException()
        {
            var id = "errorIdd";
            var cacheKey = $"Cat:{id}";

            object dummy;
            _mockMemoryCache.Setup(mc => mc.TryGetValue(cacheKey, out dummy)).Returns(false);

            _mockCatService.Setup(s => s.GetCatByIdAsync(id)).ThrowsAsync(new Exception("fail"));

            var controller = CreateController();

            var result = await controller.Get(id);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task FetchCats_ReturnsOk_WhenCatsFetchedAndSaved()
        {
            var fetchedCats = new List<CatImageDto> {
                new() { Id = "abcd" },
                new() { Id = "efgh"},
                new() { Id = "abcd" }
            };

            _mockCatApiService.Setup(s => s.GetCatImagesAsync()).ReturnsAsync(fetchedCats);

            _mockCatService.Setup(s => s.CreateCatsAsync(It.IsAny<List<CatImageDto>>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController();

            var result = await controller.FetchCats();

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task FetchCats_Returns500_WhenNoCatsReturned()
        {
            _mockCatApiService.Setup(s => s.GetCatImagesAsync()).ReturnsAsync(new List<CatImageDto>());

            var controller = CreateController();

            var result = await controller.FetchCats();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
