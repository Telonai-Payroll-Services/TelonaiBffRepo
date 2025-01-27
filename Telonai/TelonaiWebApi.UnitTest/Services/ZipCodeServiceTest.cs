using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Services;
using Xunit;
using System.Net.Sockets;
using TelonaiWebApi.Models;

namespace TelonaiWebAPI.UnitTest.Services
{
    public class ZipCodeServiceTest
    {
        public Mock<DataContext> _mockContext;
        public Mock<IMapper> _mockMapper;
        public ZipcodeService _mockZipcodeService;

        public ZipCodeServiceTest()
        {
            _mockContext = new Mock<DataContext>();
            _mockMapper = new Mock<IMapper>();
            _mockZipcodeService = new ZipcodeService(_mockContext.Object, _mockMapper.Object);
        }

        [Fact]
        public async void GetByCityId_ProvidingExistingCity_ReturnZipCodeInformaiton()
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
            var zipcodeList = new List<Zipcode>
            {
                new Zipcode 
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Zipcode>>();
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.Provider).Returns(zipcodeList.Provider);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.Expression).Returns(zipcodeList.Expression);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.ElementType).Returns(zipcodeList.ElementType);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.GetEnumerator()).Returns(zipcodeList.GetEnumerator());

            _mockContext.Setup(c => c.Zipcode).Returns(mockSet.Object);


            //Act
            var result =  _mockZipcodeService.GetByCityId(city.Id);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(zipcodeList.First(), result.First());
        }

        [Fact]
        public async void GetByCityId_ProvidingNonExistingCity_ReturnNullZipCodeInformaiton()
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
            var zipcodeList = new List<Zipcode>
            {
                new Zipcode
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Zipcode>>();
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.Provider).Returns(zipcodeList.Provider);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.Expression).Returns(zipcodeList.Expression);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.ElementType).Returns(zipcodeList.ElementType);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.GetEnumerator()).Returns(zipcodeList.GetEnumerator());

            _mockContext.Setup(c => c.Zipcode).Returns(mockSet.Object);


            //Act
            var result = _mockZipcodeService.GetByCityId(12);

            //Assert
            Assert.True(result.Count == 0);
        }

        [Fact]
        public async void GetByZipcodeAndCountryId_ProvidingExistingZipCodeIdAndCountryId_ReturnTheFirstZipcode()
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
            var zipcodeList = new List<Zipcode>
            {
                new Zipcode
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Zipcode>>();
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.Provider).Returns(zipcodeList.Provider);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.Expression).Returns(zipcodeList.Expression);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.ElementType).Returns(zipcodeList.ElementType);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.GetEnumerator()).Returns(zipcodeList.GetEnumerator());

            _mockContext.Setup(c => c.Zipcode).Returns(mockSet.Object);

            //Act
            var result = _mockZipcodeService.GetByZipcodeAndCountryId("2890", country.Id);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(zipcodeList.First(), result.First());
        }

        [Fact]
        public async void GetByZipcodeAndCountryId_ProvidingNonExistingZipCodeIdAndCountryId_ReturnNullZipcode()
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
            var zipcodeList = new List<Zipcode>
            {
                new Zipcode
                {
                    Id = 1,
                    Code = "2890",
                    CityId = city.Id,
                    City = city
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Zipcode>>();
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.Provider).Returns(zipcodeList.Provider);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.Expression).Returns(zipcodeList.Expression);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.ElementType).Returns(zipcodeList.ElementType);
            mockSet.As<IQueryable<Zipcode>>().Setup(m => m.GetEnumerator()).Returns(zipcodeList.GetEnumerator());

            _mockContext.Setup(c => c.Zipcode).Returns(mockSet.Object);

            //Act
            var result = _mockZipcodeService.GetByZipcodeAndCountryId("2891", country.Id);

            //Assert
            Assert.True(result.Count() == 0);
        }

        [Fact]
        public async void GetById_ProvidingExistingZipCode_ReturenZipcodeObject()
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
            var zipCode = new Zipcode
            {
                Id = 1,
                Code = "2890",
                CityId = city.Id,
                City = city
            };

            _mockContext.Setup(c => c.Zipcode.Find(1)).Returns(zipCode);


            //Act
            var result = _mockZipcodeService.GetById(1);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(zipCode, result);
        }

        [Fact]
        public async void GetById_ProvidingNonExistingZipCodeId_ThrowsKeyNotFoundException()
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
            var zipCode = new Zipcode
            {
                Id = 1,
                Code = "2890",
                CityId = city.Id,
                City = city
            };

            _mockContext.Setup(c => c.Zipcode.Find(2)).Returns((Zipcode)null);

            //Act
            var exception = Assert.Throws<KeyNotFoundException>(() =>
                 _mockZipcodeService.GetById(1)
             );

            //Assert
            Assert.Equal("Zipcode not found", exception.Message);
        }

        [Fact]
        public async void Delete_DeleteExistingZipCode_ReturnsTrue()
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
            var zipCode = new Zipcode
            {
                Id = 1,
                Code = "2890",
                CityId = city.Id,
                City = city
            };

            _mockContext.Setup(c => c.Zipcode.Find(1)).Returns(zipCode);

            //Act
            await _mockZipcodeService.Delete(1);

            //Assert
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }
        [Fact]
        public async void Delete_DeleteNonExistingZipCode_ReturnsTrue()
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
            var zipCode = new Zipcode
            {
                Id = 1,
                Code = "2890",
                CityId = city.Id,
                City = city
            };

            _mockContext.Setup(c => c.Zipcode.Find(1)).Returns((Zipcode)null);

            //Act
            var exception = Assert.Throws<KeyNotFoundException>(() =>
                _mockZipcodeService.GetById(4)
             );

            //Assert
            Assert.Equal("Zipcode not found", exception.Message);
        }

        [Fact]  
        public async void GetModelByZipcodeAndCountryId_ProvidingExistinZipCodeAndCountryId_ReturnZipCodeModel()
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
            
            var countryModelList  = new List<CountryModel>()
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
                Name = "North Carolina"
            };

            var cityModel = new CityModel()
            {
                Id = 1,
                Name = "Sharlott",
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
            var countyList = new List<int>();
            countyList.Add(county.Id);
            var counties = new List<County>
            {
                new County
                {
                    Id= 1,
                    Name="Mecklenburg County",
                    CountryId = country.Id,
                    StateId=state.Id
                },
                new County
                {
                    Id= 2,
                    Name="Mecklenburg County",
                    CountryId = country.Id,
                    StateId=state.Id
                },
            }.AsQueryable();
            var zipcodeList = new List<Zipcode>
            {
                new Zipcode
                {
                    Id = 1,
                    Code = "2890",
                    CountyId = countyList,
                    CityId = city.Id,
                    City = city
                },
                new Zipcode
                {
                    Id = 2,
                    CountyId = countyList,
                    Code = "2891",
                    CityId = city.Id,
                    City = city
                }
            }.AsQueryable();
            
            var mockZipCodeSet = new Mock<DbSet<Zipcode>>();
            mockZipCodeSet.As<IQueryable<Zipcode>>().Setup(m => m.Provider).Returns(zipcodeList.Provider);
            mockZipCodeSet.As<IQueryable<Zipcode>>().Setup(m => m.Expression).Returns(zipcodeList.Expression);
            mockZipCodeSet.As<IQueryable<Zipcode>>().Setup(m => m.ElementType).Returns(zipcodeList.ElementType);
            mockZipCodeSet.As<IQueryable<Zipcode>>().Setup(m => m.GetEnumerator()).Returns(zipcodeList.GetEnumerator());
            _mockContext.Setup(c => c.Zipcode).Returns(mockZipCodeSet.Object);

            var mockCountySet = new Mock<DbSet<County>>();
            mockCountySet.As<IQueryable<County>>().Setup(m => m.Provider).Returns(counties.Provider);
            mockCountySet.As<IQueryable<County>>().Setup(m => m.Expression).Returns(counties.Expression);
            mockCountySet.As<IQueryable<County>>().Setup(m => m.ElementType).Returns(counties.ElementType);
            mockCountySet.As<IQueryable<County>>().Setup(m => m.GetEnumerator()).Returns(counties.GetEnumerator());
            _mockContext.Setup(c => c.County).Returns(mockCountySet.Object);

            _mockMapper.Setup(m => m.Map<ZipcodeModel>(It.IsAny<Zipcode>())).Returns(zipCodeModel);

            //Act
            var result = _mockZipcodeService.GetModelByZipcodeAndCountryId("2890", 1);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal(zipCodeModel, result.First());
        }

        [Fact]
        public async void GetModelByZipcodeAndCountryId_ProvidingNonExistinZipCodeAndCountryId_ReturnNull()
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
                Name = "North Carolina"
            };

            var cityModel = new CityModel()
            {
                Id = 1,
                Name = "Sharlott",
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
            var countyList = new List<int>();
            countyList.Add(county.Id);
            var counties = new List<County>
            {
                new County
                {
                    Id= 1,
                    Name="Mecklenburg County",
                    CountryId = country.Id,
                    StateId=state.Id
                },
                new County
                {
                    Id= 2,
                    Name="Mecklenburg County",
                    CountryId = country.Id,
                    StateId=state.Id
                },
            }.AsQueryable();
            var zipcodeList = new List<Zipcode>
            {
                new Zipcode
                {
                    Id = 1,
                    Code = "2890",
                    CountyId = countyList,
                    CityId = city.Id,
                    City = city
                },
                new Zipcode
                {
                    Id = 2,
                    CountyId = countyList,
                    Code = "2891",
                    CityId = city.Id,
                    City = city
                }
            }.AsQueryable();

            var mockZipCodeSet = new Mock<DbSet<Zipcode>>();
            mockZipCodeSet.As<IQueryable<Zipcode>>().Setup(m => m.Provider).Returns(zipcodeList.Provider);
            mockZipCodeSet.As<IQueryable<Zipcode>>().Setup(m => m.Expression).Returns(zipcodeList.Expression);
            mockZipCodeSet.As<IQueryable<Zipcode>>().Setup(m => m.ElementType).Returns(zipcodeList.ElementType);
            mockZipCodeSet.As<IQueryable<Zipcode>>().Setup(m => m.GetEnumerator()).Returns(zipcodeList.GetEnumerator());
            _mockContext.Setup(c => c.Zipcode).Returns(mockZipCodeSet.Object);

            var mockCountySet = new Mock<DbSet<County>>();
            mockCountySet.As<IQueryable<County>>().Setup(m => m.Provider).Returns(counties.Provider);
            mockCountySet.As<IQueryable<County>>().Setup(m => m.Expression).Returns(counties.Expression);
            mockCountySet.As<IQueryable<County>>().Setup(m => m.ElementType).Returns(counties.ElementType);
            mockCountySet.As<IQueryable<County>>().Setup(m => m.GetEnumerator()).Returns(counties.GetEnumerator());
            _mockContext.Setup(c => c.County).Returns(mockCountySet.Object);

            _mockMapper.Setup(m => m.Map<ZipcodeModel>(It.IsAny<Zipcode>())).Returns(zipCodeModel);

            //Act
            var result = _mockZipcodeService.GetModelByZipcodeAndCountryId("2880", 11);

            //Assert
            Assert.True(result.Count==0);
        }
    }
}
