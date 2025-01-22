using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Services
{
    public class CityControllerTests
    {
        private readonly Mock<ICityService> _mockCityService;
        private readonly Mock<IMapper> _mockMapper;
        public CityControllerTests()
        {
            _mockCityService = new Mock<ICityService>();
            _mockMapper = new Mock<IMapper>(); 
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

            };

            var cities = new List<City>
            {
                new City { Id = 1, Name = "City A" },
                new City { Id = 2, Name = "City B" }
            };
            var citiesModel = new List<CityModel>
            {
                new CityModel { Id = 1, Name = "City A", State = state, Countrys =  },
                new CityModel { Id = 2, Name = "City B", State = state, CountryId = 1}
            };
            _mockCityService.Setup(x => x.GetByCountryId(1)).Returns(cities);
            _mockMapper.Setup(x => x.Map<IList<CityModel>>(cities)).Returns(citiesModel);
            //Act
            var result = _mockCityService.Object.GetByCountryId(1);
            //Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
            var returnedCities = Assert.IsType<List<CityModel>>(okResult.Value);
            Assert.Equal(citiesModel, returnedCities);
        }
    }
}
