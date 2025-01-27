using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Services
{
    public class CountryServiceTests
    {
        public Mock<DataContext> _mockContext;
        public Mock<IMapper> _mockMapper;
        public CountryService _mockCountryService;

        public CountryServiceTests()
        {
            _mockContext = new Mock<DataContext>();
            _mockMapper = new Mock<IMapper>();
            _mockCountryService = new CountryService(_mockContext.Object);
        }

        [Fact]
        public void GetByCountryId_WhenCalled_ReturnsCountry()
        {
            // Arrange
            var countryId = 1;
            var countries = new List<Country> {
                new Country { Id = 1, Name = "USA" },
                new Country { Id = 2, Name = "Ethiopia" }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Country>>();
            mockSet.As<IQueryable<Country>>().Setup(m => m.Provider).Returns(countries.Provider);
            mockSet.As<IQueryable<Country>>().Setup(m => m.Expression).Returns(countries.Expression);
            mockSet.As<IQueryable<Country>>().Setup(m => m.ElementType).Returns(countries.ElementType);
            mockSet.As<IQueryable<Country>>().Setup(m => m.GetEnumerator()).Returns(countries.GetEnumerator());
            _mockContext.Setup(c => c.Country.Find(countryId)).Returns(mockSet.Object.First());
            
            // Act
            var result = _mockCountryService.GetById(countryId);

            // Assert
            Assert.Equal(countries.First(), result);
        }

        [Fact]
        public void GetByCountryId_ShouldThrowKeyNotFoundException_WhenCountryDoesNotExist()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Country>>();
            var mockContext = new Mock<DbContext>();

            mockSet.Setup(m => m.Find(3)).Returns((Country)null);
            _mockContext.Setup(c => c.Country).Returns(mockSet.Object);

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _mockCountryService.GetById(3));
            Assert.Equal("Country not found", exception.Message);
        }

        [Fact]
        public async void GetByName_WhenPassingExistingCountryName_ReturntCountryObject()
        {
            // Arrange
            var countryId = 1;
            var countries = new List<Country> {
                new Country { Id = 1, Name = "USA" },
                new Country { Id = 2, Name = "Ethiopia" }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Country>>();
            mockSet.As<IQueryable<Country>>().Setup(m => m.Provider).Returns(countries.Provider);
            mockSet.As<IQueryable<Country>>().Setup(m => m.Expression).Returns(countries.Expression);
            mockSet.As<IQueryable<Country>>().Setup(m => m.ElementType).Returns(countries.ElementType);
            mockSet.As<IQueryable<Country>>().Setup(m => m.GetEnumerator()).Returns(countries.GetEnumerator());

            _mockContext.Setup(c => c.Set<Country>()).Returns(mockSet.Object);
            _mockContext.Setup(c => c.Country).Returns(mockSet.Object);

            // Act
            var result = _mockCountryService.GetByName("USA");

            //Assert
            Assert.Equal(countries.First(),result);
        }

        [Fact]
        public async void GetByName_WhenPassingNonExistingCountryName_ReturntCountryObject()
        {
            // Arrange
            var countryId = 1;
            var countries = new List<Country> {
                new Country { Id = 1, Name = "USA" },
                new Country { Id = 2, Name = "Ethiopia" }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Country>>();
            mockSet.As<IQueryable<Country>>().Setup(m => m.Provider).Returns(countries.Provider);
            mockSet.As<IQueryable<Country>>().Setup(m => m.Expression).Returns(countries.Expression);
            mockSet.As<IQueryable<Country>>().Setup(m => m.ElementType).Returns(countries.ElementType);
            mockSet.As<IQueryable<Country>>().Setup(m => m.GetEnumerator()).Returns(countries.GetEnumerator());

            _mockContext.Setup(c => c.Set<Country>()).Returns(mockSet.Object);
            _mockContext.Setup(c => c.Country).Returns(mockSet.Object);

            // Act
            var result = _mockCountryService.GetByName("Canada");

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async void Delete_WhenPassingExistingCountryId_ReturntCountryObject()
        {
            // Arrange
            var countries = new List<Country> {
                new Country { Id = 1, Name = "USA" },
                new Country { Id = 2, Name = "Ethiopia" }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Country>>();
            mockSet.As<IQueryable<Country>>().Setup(m => m.Provider).Returns(countries.Provider);
            mockSet.As<IQueryable<Country>>().Setup(m => m.Expression).Returns(countries.Expression);
            mockSet.As<IQueryable<Country>>().Setup(m => m.ElementType).Returns(countries.ElementType);
            mockSet.As<IQueryable<Country>>().Setup(m => m.GetEnumerator()).Returns(countries.GetEnumerator());

            _mockContext.Setup(c => c.Country.Find(1)).Returns(countries.First());

            // Act
            _mockCountryService.Delete(1);

            //Assert
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Fact]
        public async void Delete_WhenPassingNonExistingCountryId_ReturntException()
        {
            // Arrange
            var countries = new List<Country> {
                new Country { Id = 1, Name = "USA" },
                new Country { Id = 2, Name = "Ethiopia" }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Country>>();
            mockSet.As<IQueryable<Country>>().Setup(m => m.Provider).Returns(countries.Provider);
            mockSet.As<IQueryable<Country>>().Setup(m => m.Expression).Returns(countries.Expression);
            mockSet.As<IQueryable<Country>>().Setup(m => m.ElementType).Returns(countries.ElementType);
            mockSet.As<IQueryable<Country>>().Setup(m => m.GetEnumerator()).Returns(countries.GetEnumerator());
            _mockContext.Setup(c => c.Country.Find(4)).Returns((Country)null);

            //Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _mockCountryService.Delete(3));
            _mockContext.Verify(m => m.SaveChanges(), Times.Never);
        }

    }
}
