using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using StealTheCats.Common;
using StealTheCats.Entities.DataTransferObjects;
using StealTheCats.Entities.Models;
using StealTheCats.Interfaces;
using StealTheCats.Services;

namespace StealCats.test.Services
{
    public class CatServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly Mock<ILogger<CatService>> _mockLogger;
        private readonly CacheInvalidationToken _token;

        public CatServiceTests()
        {
            _mockLogger = new Mock<ILogger<CatService>>();
            _mockMapper = new Mock<IMapper>();
            _mockMemoryCache = new Mock<IMemoryCache>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _token = new CacheInvalidationToken();
        }

        private CatService CreateService()
        {
            return new CatService(
                    _mockUnitOfWork.Object,
                    _mockMapper.Object,
                    _mockMemoryCache.Object,
                    _token,
                    _mockLogger.Object
                );
        }

        [Fact]
        public async Task GetCatByIdAsync_ReturnsCat_WhenExists()
        {
            var catId = "abcd";
            var expectedCat = new Cat { CatId = catId };
            var cats = new List<Cat> { expectedCat }.AsQueryable();
            var mockCatQueryable = cats.BuildMockDbSet();
            var mockRepo = new Mock<IRepository<Cat>>();
            mockRepo.Setup(r => r.GetByCondition(It.IsAny<Expression<Func<Cat, bool>>>()))
                    .Returns(mockCatQueryable.Object);

            _mockUnitOfWork.Setup(u => u.GetRepository<Cat>()).Returns(mockRepo.Object);

            var service = CreateService();

            var result = await service.GetCatByIdAsync(catId);

            Assert.Equal(expectedCat, result);
        }

        [Fact]
        public async Task GetCatsAsync_ReturnsMappedPagedList()
        {
            var cats = new List<Cat> { new Cat { CatId = "abcd" }, new Cat { CatId = "efgh" } };
            var catDtos = new List<CatDto> { new CatDto { CatId = "abcd" }, new CatDto { CatId = "efgh" } };

            var pagedCats = new PagedList<Cat>(cats, 2, 1, 10);
            var pagedDto = new PagedList<CatDto>(catDtos, 2, 1, 10);

            var mockRepo = new Mock<IRepository<Cat>>();
            mockRepo.Setup(r => r.GetAll(false)).Returns(cats.AsQueryable().BuildMockDbSet().Object);

            _mockUnitOfWork.Setup(u => u.GetRepository<Cat>()).Returns(mockRepo.Object);
            _mockMapper.Setup(m => m.Map<List<CatDto>>(cats)).Returns(catDtos);

            var service = CreateService();

            var result = await service.GetCatsAsync(new CatParameters());

            Assert.Equal(pagedDto.TotalCount, result.TotalCount);
            Assert.Equal(2, result.Count);
        }


        [Fact]
        public async Task CreateCatsAsync_CreatesAndUpdatesCats_WhenValidDataProvided()
        {
            var catImageDto = new CatImageDto
            {
                Id = "abcd",
                Breeds = new List<CatBreedDto> { new CatBreedDto { Temperament = "Active" } }
            };
            var catImages = new List<CatImageDto> { catImageDto };
            var cat = new Cat()
            {
                CatId = "abcd",
                Image = [],
                Created = DateTime.UtcNow,
                Height = 100,
                Width = 100
            };

            var existingTags = new List<Tag>
            {
                new Tag { Name = "Active" },
                new Tag { Name = "Happy" }
            };

            var catsDict = new Dictionary<string, Cat>
            {
                { "efgh", new Cat { CatId = "efgh" } }
            };


            object dummyCat = catsDict;
            _mockMemoryCache.Setup(m => m.TryGetValue("CatsDict", out dummyCat)).Returns(true);

            object dummyTag = existingTags;
            _mockMemoryCache.Setup(m => m.TryGetValue("Tags", out dummyTag)).Returns(true);

            var mockCatRepo = new Mock<IRepository<Cat>>();
            _mockUnitOfWork.Setup(u => u.GetRepository<Cat>()).Returns(mockCatRepo.Object);

            var mockTagRepo = new Mock<IRepository<Tag>>();
            _mockUnitOfWork.Setup(u => u.GetRepository<Tag>()).Returns(mockTagRepo.Object);


            mockCatRepo.Setup(r => r.Create(It.IsAny<Cat>())).Verifiable();
            mockCatRepo.Setup(r => r.Update(It.IsAny<Cat>())).Verifiable();

            _mockUnitOfWork.Setup(u => u.SaveAsync()).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<Cat>(catImageDto)).Returns(cat);

            var service = CreateService();

            await service.CreateCatsAsync(catImages);

            mockCatRepo.Verify(r => r.Create(It.IsAny<Cat>()), Times.Once);
            mockCatRepo.Verify(r => r.Update(It.IsAny<Cat>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
        }
    }
}
