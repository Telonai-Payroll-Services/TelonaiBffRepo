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
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Services
{
    public class CityServiceTests
    {
        public Mock<DataContext> _mockContext;
        public CountyService _mockCityService;
        public CityServiceTests() 
        {
            _mockContext = new Mock<DataContext>();
            _mockCityService = new CountyService(_mockContext.Object);
        }

        [Fact]
        public void GetByCountryId_WhenCityExistWithCountryId_ReturnsCities()
        {
            // Arrange
            var countryId = 1;
            var cities = new List<City> {
                new City {
                    Id = 1,
                    Name = "Addis Ababa",
                    StateId=1,
                    State = new State
                    {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                },
                new City {
                    Id = 2,
                    Name = "Dire Dawa",
                    StateId=4,
                    State = new State {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<City>>();
            mockSet.As<IQueryable<City>>().Setup(m => m.Provider).Returns(cities.Provider);
            mockSet.As<IQueryable<City>>().Setup(m => m.Expression).Returns(cities.Expression);
            mockSet.As<IQueryable<City>>().Setup(m => m.ElementType).Returns(cities.ElementType);
            mockSet.As<IQueryable<City>>().Setup(m => m.GetEnumerator()).Returns(cities.GetEnumerator());
            _mockContext.Setup(c => c.City).Returns(mockSet.Object);

            // Act
            var result = _mockCityService.GetByCountryId(countryId);

            // Assert
            Assert.Equal(cities, result);
        }

        [Fact]
        public void GetByCountryId_WhenCityNotExistWithCountryId_ReturnsEmptyCityList()
        {
            // Arrange
            var countryId = 1;
            var cities = new List<City> {
                new City {
                    Id = 1,
                    Name = "Addis Ababa",
                    StateId=1,
                    State = new State
                    {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                },
                new City {
                    Id = 2,
                    Name = "Dire Dawa",
                    StateId=4,
                    State = new State {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<City>>();
            mockSet.As<IQueryable<City>>().Setup(m => m.Provider).Returns(cities.Provider);
            mockSet.As<IQueryable<City>>().Setup(m => m.Expression).Returns(cities.Expression);
            mockSet.As<IQueryable<City>>().Setup(m => m.ElementType).Returns(cities.ElementType);
            mockSet.As<IQueryable<City>>().Setup(m => m.GetEnumerator()).Returns(cities.GetEnumerator());
            _mockContext.Setup(c => c.City).Returns(mockSet.Object);

            // Act
            var result = _mockCityService.GetByCountryId(100);

            // Assert
            Assert.Equal(result.Count(), 0);
        }
        
        [Fact]
        public void GetByStateId_WhenCityExistWithStateId_ReturnsCityList() 
        {
            // Arrange
            var stateId = 1;
            var cities = new List<City> {
                new City {
                    Id = 1,
                    Name = "Kazanchize",
                    StateId=1,
                    State = new State
                    {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                },
                new City {
                    Id = 2,
                    Name = "Pissa",
                    StateId=4,
                    State = new State {
                        Id = 2,
                        Name = "Dire Dawa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<City>>();
            mockSet.As<IQueryable<City>>().Setup(m => m.Provider).Returns(cities.Provider);
            mockSet.As<IQueryable<City>>().Setup(m => m.Expression).Returns(cities.Expression);
            mockSet.As<IQueryable<City>>().Setup(m => m.ElementType).Returns(cities.ElementType);
            mockSet.As<IQueryable<City>>().Setup(m => m.GetEnumerator()).Returns(cities.GetEnumerator());
            _mockContext.Setup(c => c.City).Returns(mockSet.Object);

            // Act
            var result = _mockCityService.GetByStateId(stateId);

            // Assert
            Assert.Equal(cities.First(), result.First());
        }

        [Fact]
        public void GetByStateId_WhenCityNotExistWithStateId_ReturnsEmptyCityList()
        {
            // Arrange
            var stateId = 1;
            var cities = new List<City> {
                new City {
                    Id = 1,
                    Name = "Kazanchize",
                    StateId=1,
                    State = new State
                    {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                },
                new City {
                    Id = 2,
                    Name = "Pissa",
                    StateId=4,
                    State = new State {
                        Id = 2,
                        Name = "Dire Dawa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<City>>();
            mockSet.As<IQueryable<City>>().Setup(m => m.Provider).Returns(cities.Provider);
            mockSet.As<IQueryable<City>>().Setup(m => m.Expression).Returns(cities.Expression);
            mockSet.As<IQueryable<City>>().Setup(m => m.ElementType).Returns(cities.ElementType);
            mockSet.As<IQueryable<City>>().Setup(m => m.GetEnumerator()).Returns(cities.GetEnumerator());
            _mockContext.Setup(c => c.City).Returns(mockSet.Object);

            // Act
            var result = _mockCityService.GetByStateId(100);

            // Assert
            Assert.Equal(result.Count(), 0);
        }

        [Fact]
        public void GetById_WhenCityExistWithId_ReturnsCityObject()
        {
            // Arrange
            var cities = new List<City> {
                new City {
                    Id = 1,
                    Name = "Kazanchize",
                    StateId=1,
                    State = new State
                    {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                },
                new City {
                    Id = 2,
                    Name = "Pissa",
                    StateId=4,
                    State = new State {
                        Id = 2,
                        Name = "Dire Dawa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<City>>();
            mockSet.As<IQueryable<City>>().Setup(m => m.Provider).Returns(cities.Provider);
            mockSet.As<IQueryable<City>>().Setup(m => m.Expression).Returns(cities.Expression);
            mockSet.As<IQueryable<City>>().Setup(m => m.ElementType).Returns(cities.ElementType);
            mockSet.As<IQueryable<City>>().Setup(m => m.GetEnumerator()).Returns(cities.GetEnumerator());
            _mockContext.Setup(c => c.City).Returns(mockSet.Object);

            // Act
            var result = _mockCityService.GetById(1);
            // Assert
            Assert.Equal(cities.First(), result);
        }

        [Fact]
        public void GetById_WhenNoCityExistWithId_ReturnsException()
        {
            // Arrange
            var cities = new List<City> {
                new City {
                    Id = 1,
                    Name = "Kazanchize",
                    StateId=1,
                    State = new State
                    {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                },
                new City {
                    Id = 2,
                    Name = "Pissa",
                    StateId=4,
                    State = new State {
                        Id = 2,
                        Name = "Dire Dawa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<City>>();
            mockSet.As<IQueryable<City>>().Setup(m => m.Provider).Returns(cities.Provider);
            mockSet.As<IQueryable<City>>().Setup(m => m.Expression).Returns(cities.Expression);
            mockSet.As<IQueryable<City>>().Setup(m => m.ElementType).Returns(cities.ElementType);
            mockSet.As<IQueryable<City>>().Setup(m => m.GetEnumerator()).Returns(cities.GetEnumerator());
            _mockContext.Setup(c => c.City).Returns(mockSet.Object);

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _mockCityService.GetById(3));
            Assert.Equal("City not found", exception.Message);
        }
        [Fact]
        public void Delete_WhenCityExistWithId_ReturnsException()
        {
            // Arrange
            var cities = new List<City> {
                new City {
                    Id = 1,
                    Name = "Kazanchize",
                    StateId=1,
                    State = new State
                    {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                },
                new City {
                    Id = 2,
                    Name = "Pissa",
                    StateId=4,
                    State = new State {
                        Id = 2,
                        Name = "Dire Dawa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<City>>();
            mockSet.As<IQueryable<City>>().Setup(m => m.Provider).Returns(cities.Provider);
            mockSet.As<IQueryable<City>>().Setup(m => m.Expression).Returns(cities.Expression);
            mockSet.As<IQueryable<City>>().Setup(m => m.ElementType).Returns(cities.ElementType);
            mockSet.As<IQueryable<City>>().Setup(m => m.GetEnumerator()).Returns(cities.GetEnumerator());
            _mockContext.Setup(c => c.City).Returns(mockSet.Object);

            // Act
            _mockCityService.Delete(1);

            //Assert
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Delete_WhenNoCityExistWithId_ReturnsException()
        {
            // Arrange
            var cities = new List<City> {
                new City {
                    Id = 1,
                    Name = "Kazanchize",
                    StateId=1,
                    State = new State
                    {
                        Id = 1,
                        Name = "Addis Ababa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                },
                new City {
                    Id = 2,
                    Name = "Pissa",
                    StateId=4,
                    State = new State {
                        Id = 2,
                        Name = "Dire Dawa",
                        CountryId = 1,
                        Country = new Country { Id = 1, Name = "Ethiopia" }
                    }
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<City>>();
            mockSet.As<IQueryable<City>>().Setup(m => m.Provider).Returns(cities.Provider);
            mockSet.As<IQueryable<City>>().Setup(m => m.Expression).Returns(cities.Expression);
            mockSet.As<IQueryable<City>>().Setup(m => m.ElementType).Returns(cities.ElementType);
            mockSet.As<IQueryable<City>>().Setup(m => m.GetEnumerator()).Returns(cities.GetEnumerator());
            _mockContext.Setup(c => c.City).Returns(mockSet.Object);

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _mockCityService.Delete(3));
            Assert.Equal("City not found", exception.Message);
        }
    }
}
