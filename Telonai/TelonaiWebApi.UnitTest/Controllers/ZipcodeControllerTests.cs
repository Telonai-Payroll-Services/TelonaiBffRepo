using AutoMapper;
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
    public class ZipcodeControllerTests
    {
        public Mock<IZipcodeService> _mockService;
        public Mock<IMapper> _mockMapper;
        public ZipcodesController _zipcodesController;
        public ZipcodeControllerTests()
        {
            _mockService = new Mock<IZipcodeService>();
            _mockMapper = new Mock<IMapper>();
            _zipcodesController = new ZipcodesController(_mockService.Object,_mockMapper.Object);
        }

        [Fact]
        public async void GetZipByCityId_ProvidingExistingCityIdOfZipcode_ReturnsOKResult()
        {
            //Arrange
            var country = new Country()
            {
                Id = 1,
                CountryCode = "USA",
                Name = "United States of America",
                PhoneCountryCode = "+1"
            };
            var state = new State()
            {
                Id = 1,
                CountryId = country.Id,
                Country = country,
                Name = "North Carolina",
                StateCode = "NC"
            };
            var city = new City()
            {
                Id = 1,
                Name = "Charlott",
                State = state,
                StateId = state.Id
            };
            var county = new County()
            {
                Id = 1,
                Name = "Mecklenburg",
                StateId = state.Id,
                CountryId = country.Id,
            };
            var countyList = new List<int>();
            countyList.Add(county.Id);
            var zipcode = new List<Zipcode>() 
            { 
                new  Zipcode()
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city,
                    CountyId = countyList
                }
            };

            var countryModelList = new List<CountryModel>()
            {
                new CountryModel
                {
                    Id = 1,
                    CountryCode = "USA",
                    Name = "United States of America",
                    PhoneCountryCode = "+1"
                }
            };
            var stateModel = new StateModel()
            {
                Id = 1,
                Country = "USA",
                CountryId = country.Id,
                Name = "North Carolina",
                
            };

            var cityModel = new CityModel()
            {
                Id = 1,
                Countrys = countryModelList,
                Name = "Sharlott",
                State = stateModel
            };

            var countyModelList = new List<CountyModel>()
            {
                new CountyModel
                {
                    Id= 1,
                    Name="Mecklenburg County"
                }
            };

            var zipCodeModel = new List<ZipcodeModel>()
            {
                new ZipcodeModel()
                {
                    Id = 1,
                    City = cityModel,
                    Code = "2890",
                    Counties = countyModelList
                }
            };

           
            _mockService.Setup(x => x.GetByCityId(zipcode.First().Id)).Returns(zipcode);
            _mockMapper.Setup(m => m.Map<List<ZipcodeModel>>(It.IsAny<List<Zipcode>>())).Returns(zipCodeModel);

            //Act
            var result = _zipcodesController.GetByCityId(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedZipCode = Assert.IsType<List<ZipcodeModel>>(okResult.Value);
            Assert.Equal(zipCodeModel.First(), returnedZipCode.First());
        }

        [Fact]
        public async void GetZipByCityId_ProvidingNonExistingCityIdOfZipcode_ReturnsNotFoundResult()
        {
            //Arrange
            var country = new Country()
            {
                Id = 1,
                CountryCode = "USA",
                Name = "United States of America",
                PhoneCountryCode = "+1"
            };
            var state = new State()
            {
                Id = 1,
                CountryId = country.Id,
                Country = country,
                Name = "North Carolina",
                StateCode = "NC"
            };
            var city = new City()
            {
                Id = 1,
                Name = "Charlott",
                State = state,
                StateId = state.Id
            };
            var county = new County()
            {
                Id = 1,
                Name = "Mecklenburg",
                StateId = state.Id,
                CountryId = country.Id,
            };
            var countyList = new List<int>();
            countyList.Add(county.Id);
            var zipcode = new List<Zipcode>()
            {
                new  Zipcode()
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city,
                    CountyId = countyList
                }
            };

            var countryModelList = new List<CountryModel>()
            {
                new CountryModel
                {
                    Id = 1,
                    CountryCode = "USA",
                    Name = "United States of America",
                    PhoneCountryCode = "+1"
                }
            };
            var stateModel = new StateModel()
            {
                Id = 1,
                Country = "USA",
                CountryId = country.Id,
                Name = "North Carolina",

            };

            var cityModel = new CityModel()
            {
                Id = 1,
                Countrys = countryModelList,
                Name = "Sharlott",
                State = stateModel
            };

            var countyModelList = new List<CountyModel>()
            {
                new CountyModel
                {
                    Id= 1,
                    Name="Mecklenburg County"
                }
            };

            var zipCodeModel = new List<ZipcodeModel>()
            {
                new ZipcodeModel()
                {
                    Id = 1,
                    City = cityModel,
                    Code = "2890",
                    Counties = countyModelList
                }
            };


            _mockService.Setup(x => x.GetByCityId(4)).Returns((List<Zipcode>)null);
            _mockMapper.Setup(m => m.Map<List<ZipcodeModel>>(It.IsAny<List<Zipcode>>())).Returns(zipCodeModel);

            //Act
            var result = _zipcodesController.GetByCityId(12);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void GetByZipCodeAndCountryId_ProvidingExistingCityIdOfZipcode_ReturnsOKResult()
        {
            //Arrange
            var country = new Country()
            {
                Id = 1,
                CountryCode = "USA",
                Name = "United States of America",
                PhoneCountryCode = "+1"
            };
            var state = new State()
            {
                Id = 1,
                CountryId = country.Id,
                Country = country,
                Name = "North Carolina",
                StateCode = "NC"
            };
            var city = new City()
            {
                Id = 1,
                Name = "Charlott",
                State = state,
                StateId = state.Id
            };
            var county = new County()
            {
                Id = 1,
                Name = "Mecklenburg",
                StateId = state.Id,
                CountryId = country.Id,
            };
            var countyList = new List<int>();
            countyList.Add(county.Id);
            var zipcode = new List<Zipcode>()
            {
                new  Zipcode()
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city,
                    CountyId = countyList
                }
            };

            var countryModelList = new List<CountryModel>()
            {
                new CountryModel
                {
                    Id = 1,
                    CountryCode = "USA",
                    Name = "United States of America",
                    PhoneCountryCode = "+1"
                }
            };
            var stateModel = new StateModel()
            {
                Id = 1,
                Country = "USA",
                CountryId = country.Id,
                Name = "North Carolina",

            };

            var cityModel = new CityModel()
            {
                Id = 1,
                Countrys = countryModelList,
                Name = "Sharlott",
                State = stateModel
            };

            var countyModelList = new List<CountyModel>()
            {
                new CountyModel
                {
                    Id= 1,
                    Name="Mecklenburg County"
                }
            };

            var zipCodeModel = new List<ZipcodeModel>()
            {
                new ZipcodeModel()
                {
                    Id = 1,
                    City = cityModel,
                    Code = "2890",
                    Counties = countyModelList
                }
            };

            _mockService.Setup(x => x.GetModelByZipcodeAndCountryId("2890", 1)).Returns(zipCodeModel);
            _mockMapper.Setup(m => m.Map<List<ZipcodeModel>>(It.IsAny<List<Zipcode>>())).Returns(zipCodeModel);

            var result = _zipcodesController.GetByZipCodeAndCountryId("2890", country.Id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedZipCode = Assert.IsType<List<ZipcodeModel>>(okResult.Value);
            Assert.Equal(zipCodeModel.First(), returnedZipCode.First());
        }

        [Fact]
        public async void GetByZipCodeAndCountryId_ProvidingNonExistingCityIdOfZipcode_ReturnsNotFoundResult()
        {
            //Arrange
            var country = new Country()
            {
                Id = 1,
                CountryCode = "USA",
                Name = "United States of America",
                PhoneCountryCode = "+1"
            };
            var state = new State()
            {
                Id = 1,
                CountryId = country.Id,
                Country = country,
                Name = "North Carolina",
                StateCode = "NC"
            };
            var city = new City()
            {
                Id = 1,
                Name = "Charlott",
                State = state,
                StateId = state.Id
            };
            var county = new County()
            {
                Id = 1,
                Name = "Mecklenburg",
                StateId = state.Id,
                CountryId = country.Id,
            };
            var countyList = new List<int>();
            countyList.Add(county.Id);
            var zipcode = new List<Zipcode>()
            {
                new  Zipcode()
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city,
                    CountyId = countyList
                }
            };

            var countryModelList = new List<CountryModel>()
            {
                new CountryModel
                {
                    Id = 1,
                    CountryCode = "USA",
                    Name = "United States of America",
                    PhoneCountryCode = "+1"
                }
            };
            var stateModel = new StateModel()
            {
                Id = 1,
                Country = "USA",
                CountryId = country.Id,
                Name = "North Carolina",

            };

            var cityModel = new CityModel()
            {
                Id = 1,
                Countrys = countryModelList,
                Name = "Sharlott",
                State = stateModel
            };

            var countyModelList = new List<CountyModel>()
            {
                new CountyModel
                {
                    Id= 1,
                    Name="Mecklenburg County"
                }
            };

            var zipCodeModel = new List<ZipcodeModel>()
            {
                new ZipcodeModel()
                {
                    Id = 1,
                    City = cityModel,
                    Code = "2890",
                    Counties = countyModelList
                }
            };


            _mockService.Setup(x => x.GetModelByZipcodeAndCountryId("1200",4)).Returns((List<ZipcodeModel>)null);
            _mockMapper.Setup(m => m.Map<List<ZipcodeModel>>(It.IsAny<List<Zipcode>>())).Returns(zipCodeModel);

            //Act
            var result = _zipcodesController.GetByZipCodeAndCountryId("5600",12);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void GetById_ProvidingExistingCityIdOfZipcode_ReturnsOKResult()
        {
            //Arrange
            var country = new Country()
            {
                Id = 1,
                CountryCode = "USA",
                Name = "United States of America",
                PhoneCountryCode = "+1"
            };
            var state = new State()
            {
                Id = 1,
                CountryId = country.Id,
                Country = country,
                Name = "North Carolina",
                StateCode = "NC"
            };
            var city = new City()
            {
                Id = 1,
                Name = "Charlott",
                State = state,
                StateId = state.Id
            };
            var county = new County()
            {
                Id = 1,
                Name = "Mecklenburg",
                StateId = state.Id,
                CountryId = country.Id,
            };
            var countyList = new List<int>();
            countyList.Add(county.Id);
            var zipcode = new List<Zipcode>()
            {
                new  Zipcode()
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city,
                    CountyId = countyList
                }
            };

            var countryModelList = new List<CountryModel>()
            {
                new CountryModel
                {
                    Id = 1,
                    CountryCode = "USA",
                    Name = "United States of America",
                    PhoneCountryCode = "+1"
                }
            };
            var stateModel = new StateModel()
            {
                Id = 1,
                Country = "USA",
                CountryId = country.Id,
                Name = "North Carolina",

            };

            var cityModel = new CityModel()
            {
                Id = 1,
                Countrys = countryModelList,
                Name = "Sharlott",
                State = stateModel
            };

            var countyModelList = new List<CountyModel>()
            {
                new CountyModel
                {
                    Id= 1,
                    Name="Mecklenburg County"
                }
            };

            var zipCodeModel = new ZipcodeModel()
            {
                Id = 1,
                City = cityModel,
                Code = "2890",
                Counties = countyModelList
            };


            _mockService.Setup(x => x.GetById(zipcode.First().Id)).Returns(zipcode.First());
            _mockMapper.Setup(m => m.Map<ZipcodeModel>(It.IsAny<Zipcode>())).Returns(zipCodeModel);

            //Act
            var result = _zipcodesController.GetById(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedZipCode = Assert.IsType<ZipcodeModel>(okResult.Value);
            Assert.Equal(zipCodeModel, returnedZipCode);
        }

        [Fact]
        public async void GetById_ProvidingNonExistingCityIdOfZipcode_ReturnsNotFoundResult()
        {
            //Arrange
            var country = new Country()
            {
                Id = 1,
                CountryCode = "USA",
                Name = "United States of America",
                PhoneCountryCode = "+1"
            };
            var state = new State()
            {
                Id = 1,
                CountryId = country.Id,
                Country = country,
                Name = "North Carolina",
                StateCode = "NC"
            };
            var city = new City()
            {
                Id = 1,
                Name = "Charlott",
                State = state,
                StateId = state.Id
            };
            var county = new County()
            {
                Id = 1,
                Name = "Mecklenburg",
                StateId = state.Id,
                CountryId = country.Id,
            };
            var countyList = new List<int>();
            countyList.Add(county.Id);
            var zipcode = new List<Zipcode>()
            {
                new  Zipcode()
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city,
                    CountyId = countyList
                }
            };

            var countryModelList = new List<CountryModel>()
            {
                new CountryModel
                {
                    Id = 1,
                    CountryCode = "USA",
                    Name = "United States of America",
                    PhoneCountryCode = "+1"
                }
            };
            var stateModel = new StateModel()
            {
                Id = 1,
                Country = "USA",
                CountryId = country.Id,
                Name = "North Carolina",

            };

            var cityModel = new CityModel()
            {
                Id = 1,
                Countrys = countryModelList,
                Name = "Sharlott",
                State = stateModel
            };

            var countyModelList = new List<CountyModel>()
            {
                new CountyModel
                {
                    Id= 1,
                    Name="Mecklenburg County"
                }
            };

            var zipCodeModel = new ZipcodeModel()
            {
                Id = 1,
                City = cityModel,
                Code = "2890",
                Counties = countyModelList
            };


            _mockService.Setup(x => x.GetById(12)).Returns((Zipcode)null);
            _mockMapper.Setup(m => m.Map<ZipcodeModel>(It.IsAny<Zipcode>())).Returns(zipCodeModel);

            //Act
            var result = _zipcodesController.GetById(15);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
