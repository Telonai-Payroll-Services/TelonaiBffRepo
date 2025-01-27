using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Controllers
{
    public class CountryControllerTests
    {
        private readonly Mock<ICountryService> _mockCountryService;
        private readonly Mock<IMapper> _mockMapper;
        public CountriesController _CountryController;
        public CountryControllerTests()
        {
            _mockCountryService = new Mock<ICountryService>();
            _mockMapper = new Mock<IMapper>();
            _CountryController = new CountriesController(_mockCountryService.Object, _mockMapper.Object);
        }

        [Fact]
        public void GetAll_ReturnsOkResult_WithListOfCountries()
        {
            //Arrange
            var countries = new List<Country>
            {
                new Country { Id = 1, Name = "Country A" },
                new Country { Id = 2, Name = "Country B" }
            };
            var countriesModel= new List<CountryModel>
            {
                new CountryModel { Id = 1, Name = "Country A" },
                new CountryModel { Id = 2, Name = "Country B" }
            };
            _mockCountryService.Setup(x => x.GetAll()).Returns(countries);
            _mockMapper.Setup(x => x.Map<IList<CountryModel>>(countries)).Returns(countriesModel);
            
            //Act
            var result = _CountryController.GetAll();

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCountries = Assert.IsType<List<CountryModel>>(okResult.Value);
            Assert.Equal(countriesModel, returnedCountries);
        }

        [Fact]
        public void GetById_WithProvidingExistingCountry_ReturnsOkResult()
        {
            //Arrange
            var countries = new List<Country>
            {
                new Country { Id = 1, Name = "USA" },
                new Country { Id = 2, Name = "Ethiopia" }
            };
            var countriesModel = new List<CountryModel>
            {
                new CountryModel { Id = 1, Name = "USA" },
                new CountryModel { Id = 2, Name = "Ethiopia" }
            };
            _mockCountryService.Setup(x => x.GetById(1)).Returns(countries.First());
            _mockMapper.Setup(x => x.Map<CountryModel>(countries.First())).Returns(countriesModel.First());

            //Act
            var result = _CountryController.GetById(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCountry = Assert.IsType<CountryModel>(okResult.Value);
            Assert.Equal(countriesModel.First(), returnedCountry);
        }

        [Fact]
        public void GetById_WithProvidingExistingCountry_ReturnsNull()
        {
            //Arrange
            var countries = new List<Country>
            {
                new Country { Id = 1, Name = "USA" },
                new Country { Id = 2, Name = "Ethiopia" }
            };
            var countriesModel = new List<CountryModel>
            {
                new CountryModel { Id = 1, Name = "USA" },
                new CountryModel { Id = 2, Name = "Ethiopia" }
            };
            _mockCountryService.Setup(x => x.GetById(1)).Throws(new Exception("Country not found"));

            //Act
            var result = _CountryController.GetById(91);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }
    }
}
