using Moq;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using Xunit;

namespace TelonaiWebApi.Services
{
    public class CountyServiceTests
    {
        public Mock<DataContext> _mockContext;
        public CountyService _countyService;
        public CountyServiceTests()
        {
            _mockContext = new Mock<DataContext>();
            _countyService = new CountyService(_mockContext.Object);
        }

        [Fact]
        public async void GetById_PassingNonExistingCountyId_ReturnsCountyDetail()
        {
            //Arrange
            var country = new Country
            {
                Id = 12,
                Name = "Ethiopia"
            };
            var state = new State
            {
                Id = 11,
                Name = "Addis Ababa",
                CountryId = 12,
            };
            var counties = new List<County>
            {
                new County { Id = 1, Name = "County A", StateId = 11, CountryId = 12 },
                new County { Id = 2, Name = "County B", StateId = 11, CountryId = 12 }
            }.AsQueryable();
            
            var mockSet = new Mock<DbSet<County>>();
            mockSet.As<IQueryable<County>>().Setup(m => m.Provider).Returns(counties.Provider);
            mockSet.As<IQueryable<County>>().Setup(m => m.Expression).Returns(counties.Expression);
            mockSet.As<IQueryable<County>>().Setup(m => m.ElementType).Returns(counties.ElementType);
            mockSet.As<IQueryable<County>>().Setup(m => m.GetEnumerator()).Returns(counties.GetEnumerator());
            _mockContext.Setup(c => c.County.Find(4)).Returns((County)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _countyService.GetById(3));
            Assert.Equal("County not found", exception.Message);
        }

        [Fact]
        public async void GetById_PassingExistingCountyId_ReturnsCountyDetail()
        {
            //Arrange
            var country = new Country
            {
                Id = 12,
                Name = "Ethiopia"
            };
            var state = new State
            {
                Id = 11,
                Name = "Addis Ababa",
                CountryId = 12,
            };
            var counties = new List<County>
            {
                new County { Id = 1, Name = "County A", StateId = 11, CountryId = 12 },
                new County { Id = 2, Name = "County B", StateId = 11, CountryId = 12 }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<County>>();
            mockSet.As<IQueryable<County>>().Setup(m => m.Provider).Returns(counties.Provider);
            mockSet.As<IQueryable<County>>().Setup(m => m.Expression).Returns(counties.Expression);
            mockSet.As<IQueryable<County>>().Setup(m => m.ElementType).Returns(counties.ElementType);
            mockSet.As<IQueryable<County>>().Setup(m => m.GetEnumerator()).Returns(counties.GetEnumerator());
            _mockContext.Setup(c => c.County.FindAsync(1)).ReturnsAsync(counties.First());

            // Act
            var result = await  _countyService.GetById(1);
            // Assert
            Assert.Equal(counties.First(), result);
        }

        [Fact]
        public async void GetByStateId_PassingNonExistingStateId_ReturnsEmptyListOfCounties()
        {
            //Arrange
            var country = new Country
            {
                Id = 12,
                Name = "Ethiopia"
            };
            var state = new State
            {
                Id = 11,
                Name = "Addis Ababa",
                CountryId = 12,
            };
            var counties = new List<County>
            {
                new County { Id = 1, Name = "County A", StateId = 11, CountryId = 12 },
                new County { Id = 2, Name = "County B", StateId = 11, CountryId = 12 }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<County>>();
            mockSet.As<IQueryable<County>>().Setup(m => m.Provider).Returns(counties.Provider);
            mockSet.As<IQueryable<County>>().Setup(m => m.Expression).Returns(counties.Expression);
            mockSet.As<IQueryable<County>>().Setup(m => m.ElementType).Returns(counties.ElementType);
            mockSet.As<IQueryable<County>>().Setup(m => m.GetEnumerator()).Returns(counties.GetEnumerator());
            _mockContext.Setup(c => c.County).Returns(mockSet.Object);

            // Act
            var result = await _countyService.GetByStateId(1);

            // Assert
            Assert.Equal(result.Count, 0);
        }

        [Fact]
        public async void GetByStateId_PassingExistingStateId_ReturnsListOfCounties()
        {
            //Arrange
            var country = new Country
            {
                Id = 12,
                Name = "Ethiopia"
            };
            var state = new State
            {
                Id = 11,
                Name = "Addis Ababa",
                CountryId = 12,
            };
            var counties = new List<County>
            {
                new County { Id = 1, Name = "County A", StateId = 11, CountryId = 12 },
                new County { Id = 2, Name = "County B", StateId = 11, CountryId = 12 }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<County>>();
            mockSet.As<IQueryable<County>>().Setup(m => m.Provider).Returns(counties.Provider);
            mockSet.As<IQueryable<County>>().Setup(m => m.Expression).Returns(counties.Expression);
            mockSet.As<IQueryable<County>>().Setup(m => m.ElementType).Returns(counties.ElementType);
            mockSet.As<IQueryable<County>>().Setup(m => m.GetEnumerator()).Returns(counties.GetEnumerator());
            _mockContext.Setup(c => c.County).Returns(mockSet.Object);

            // Act
            var result = await _countyService.GetByStateId(11);

            // Assert
            Assert.Equal(result.Count(), counties.Count());
        }

        [Fact]
        public async void GetByNameAndStateId_PassingNonExistingCountyName_ReturnsNull()
        {
            //Arrange
            var country = new Country
            {
                Id = 12,
                Name = "Ethiopia"
            };
            var state = new State
            {
                Id = 11,
                Name = "Addis Ababa",
                CountryId = 12,
            };
            var counties = new List<County>
            {
                new County { Id = 1, Name = "County A", StateId = 11, CountryId = 12 },
                new County { Id = 2, Name = "County B", StateId = 11, CountryId = 12 }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<County>>();
            mockSet.As<IQueryable<County>>().Setup(m => m.Provider).Returns(counties.Provider);
            mockSet.As<IQueryable<County>>().Setup(m => m.Expression).Returns(counties.Expression);
            mockSet.As<IQueryable<County>>().Setup(m => m.ElementType).Returns(counties.ElementType);
            mockSet.As<IQueryable<County>>().Setup(m => m.GetEnumerator()).Returns(counties.GetEnumerator());
            _mockContext.Setup(c => c.County).Returns(mockSet.Object);

            // Act
            var result = _countyService.GetByNameAndStateId("County C", 11);
            
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async void GetByNameAndStateId_PassingNonExistingStateId_ReturnsNull()
        {
            //Arrange
            var country = new Country
            {
                Id = 12,
                Name = "Ethiopia"
            };
            var state = new State
            {
                Id = 11,
                Name = "Addis Ababa",
                CountryId = 12,
            };
            var counties = new List<County>
            {
                new County { Id = 1, Name = "County A", StateId = 11, CountryId = 12 },
                new County { Id = 2, Name = "County B", StateId = 11, CountryId = 12 }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<County>>();
            mockSet.As<IQueryable<County>>().Setup(m => m.Provider).Returns(counties.Provider);
            mockSet.As<IQueryable<County>>().Setup(m => m.Expression).Returns(counties.Expression);
            mockSet.As<IQueryable<County>>().Setup(m => m.ElementType).Returns(counties.ElementType);
            mockSet.As<IQueryable<County>>().Setup(m => m.GetEnumerator()).Returns(counties.GetEnumerator());
            _mockContext.Setup(c => c.County).Returns(mockSet.Object);

            // Act
            var result = _countyService.GetByNameAndStateId("County A", 13);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async void GetByNameAndStateId_PassingExistingStateIdAndCountyName_ReturnsCounty()
        {
            //Arrange
            var country = new Country
            {
                Id = 12,
                Name = "Ethiopia"
            };
            var state = new State
            {
                Id = 11,
                Name = "Addis Ababa",
                CountryId = 12,
            };
            var counties = new List<County>
            {
                new County { Id = 1, Name = "County A", StateId = 11, CountryId = 12 },
                new County { Id = 2, Name = "County B", StateId = 11, CountryId = 12 }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<County>>();
            mockSet.As<IQueryable<County>>().Setup(m => m.Provider).Returns(counties.Provider);
            mockSet.As<IQueryable<County>>().Setup(m => m.Expression).Returns(counties.Expression);
            mockSet.As<IQueryable<County>>().Setup(m => m.ElementType).Returns(counties.ElementType);
            mockSet.As<IQueryable<County>>().Setup(m => m.GetEnumerator()).Returns(counties.GetEnumerator());
            _mockContext.Setup(c => c.County).Returns(mockSet.Object);

            // Act
            var result = _countyService.GetByNameAndStateId("County A", 11);

            // Assert
            Assert.Equal(counties.First(), result);
        }
    }
}
