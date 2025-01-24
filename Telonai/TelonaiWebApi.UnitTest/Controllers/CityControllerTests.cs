using AutoMapper;
using Moq;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;
using TelonaiWebApi.Controllers;

namespace TelonaiWebAPI.UnitTest.Controllers
{
    public class CityControllerTests
    {
        private readonly Mock<ICityService> _mockCityService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CitiesController _cityController;
        public CityControllerTests()
        {
            _mockCityService = new Mock<ICityService>();
            _mockMapper = new Mock<IMapper>();
            _cityController = new CitiesController(_mockCityService.Object, _mockMapper.Object);
        }

        [Fact]
        public void GetByCountryId_WithProvidingExistingCountryId_ReturnsOkResult()
        {
            //Arrange
            var state = new StateModel
            {
                Id = 1,
                Name = "North Carolina"
            };

            var county = new CountyModel
            {
                Id = 1,
                Name = "USA"
            };

            var cities = new List<City>
            {
                new City { Id = 1, Name = "City A" },
                new City { Id = 2, Name = "City B" }
            };
            var citiesModel = new List<CityModel>
            {
                new CityModel { Id = 1, Name = "City A", State = state},
                new CityModel { Id = 2, Name = "City B", State = state}
            };

            _mockCityService.Setup(x => x.GetByCountryId(1)).Returns(cities);
            _mockMapper.Setup(x => x.Map<IList<CityModel>>(cities)).Returns(citiesModel);
            //Act
            var result = _cityController.GetByCountryId(1);
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCities = Assert.IsType<List<CityModel>>(okResult.Value);
            Assert.Equal(citiesModel.Count(), returnedCities.Count());
        }
        
        [Fact]
        public void GetByCountryId_WithProvidingNonExistingCountryId_ReturnsOkResultWithNullValue()
        {
            //Arrange
            var state = new StateModel
            {
                Id = 1,
                Name = "North Carolina"
            };

            var county = new CountyModel
            {
                Id = 1,
                Name = "USA"
            };

            var cities = new List<City>
            {
                new City { Id = 1, Name = "City A" },
                new City { Id = 2, Name = "City B" }
            };
            var citiesModel = new List<CityModel>
            {
                new CityModel { Id = 1, Name = "City A", State = state},
                new CityModel { Id = 2, Name = "City B", State = state}
            };

            _mockCityService.Setup(x => x.GetByCountryId(1)).Returns((List<City>)null);
            _mockMapper.Setup(x => x.Map<IList<CityModel>>(cities)).Returns(citiesModel);
            //Act
            var result = _cityController.GetByCountryId(12);
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public void GetByStateId_ProvidingExistingStateId_ReturnOkResult()
        {
            //Arrange
            var state = new StateModel
            {
                Id = 1,
                Name = "North Carolina"
            };

            var county = new CountyModel
            {
                Id = 1,
                Name = "USA"
            };

            var cities = new List<City>
            {
                new City { Id = 1, Name = "City A" },
                new City { Id = 2, Name = "City B" }
            };
            var citiesModel = new List<CityModel>
            {
                new CityModel { Id = 1, Name = "City A", State = state},
                new CityModel { Id = 2, Name = "City B", State = state}
            };

            _mockCityService.Setup(x => x.GetByStateId(1)).Returns(cities);
            _mockMapper.Setup(x => x.Map<IList<CityModel>>(cities)).Returns(citiesModel);

            //Act
            var result = _cityController.GetByStateId(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCities = Assert.IsType<List<CityModel>>(okResult.Value);
            Assert.Equal(citiesModel.Count(), returnedCities.Count());
        }

        [Fact]
        public void GetByStateId_ProvidingNonExistingStateIdCity_ReturnOkResult()
        {
            //Arrange
            var state = new StateModel
            {
                Id = 1,
                Name = "North Carolina"
            };

            var county = new CountyModel
            {
                Id = 1,
                Name = "USA"
            };

            var cities = new List<City>
            {
                new City { Id = 1, Name = "City A" },
                new City { Id = 2, Name = "City B" }
            };
            var citiesModel = new List<CityModel>
            {
                new CityModel { Id = 1, Name = "City A", State = state},
                new CityModel { Id = 2, Name = "City B", State = state}
            };

            _mockCityService.Setup(x => x.GetByStateId(12)).Returns((List<City>)null);
            _mockMapper.Setup(x => x.Map<IList<CityModel>>(cities)).Returns(citiesModel);

            //Act
            var result = _cityController.GetByStateId(12);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public void GetById_ProvidingExistingCityId_ReturnsOkResult()
        {
            //Arrange
            var state = new StateModel
            {
                Id = 1,
                Name = "North Carolina"
            };
            var city = new City
            {
                Id = 1,
                Name = "City A"
            };
            var cityModel = new CityModel
            {
                Id = 1,
                Name = "City A",
                State = state
            };
            _mockCityService.Setup(x => x.GetById(1)).Returns(city);
            _mockMapper.Setup(x => x.Map<CityModel>(city)).Returns(cityModel);

            //Act
            var result = _cityController.GetById(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCity = Assert.IsType<CityModel>(okResult.Value);
            Assert.Equal(cityModel, returnedCity);
        }

        [Fact]
        public void GetById_ProvidingNonExistingCityId_ReturnsOkResultWithNullValue()
        {
            //Arrange
            var state = new StateModel
            {
                Id = 1,
                Name = "North Carolina"
            };
            var city = new City
            {
                Id = 1,
                Name = "City A"
            };
            var cityModel = new CityModel
            {
                Id = 1,
                Name = "City A",
                State = state
            };
            _mockCityService.Setup(x => x.GetById(1)).Returns((City)null);
            _mockMapper.Setup(x => x.Map<CityModel>(city)).Returns(cityModel);

            //Act
            var result = _cityController.GetById(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }
    }
}
