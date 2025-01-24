using Amazon.CognitoIdentityProvider.Model;
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
    public class StateServiceTests
    {
        public Mock<DataContext> _mockContext;
        public StateService _mockStateService;
        public StateServiceTests()
        {
            _mockContext = new Mock<DataContext>();
            _mockStateService = new StateService(_mockContext.Object);
        }

        [Fact]
        public void GetByCountryId_WhenCalled_ReturnsState()
        {
            // Arrange
            var countryId = 1;
            var states = new List<State> {
                new State {
                    Id = 1,
                    Name = "California",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }},
                new State {
                    Id = 2,
                    Name = "Addis Abeba",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }}
            }.AsQueryable();
            var mockSet = new Mock<DbSet<State>>();
            mockSet.As<IQueryable<State>>().Setup(m => m.Provider).Returns(states.Provider);
            mockSet.As<IQueryable<State>>().Setup(m => m.Expression).Returns(states.Expression);
            mockSet.As<IQueryable<State>>().Setup(m => m.ElementType).Returns(states.ElementType);
            mockSet.As<IQueryable<State>>().Setup(m => m.GetEnumerator()).Returns(states.GetEnumerator());
            _mockContext.Setup(c => c.State).Returns(mockSet.Object);

            // Act
            var result = _mockStateService.GetByCountryId(countryId);

            // Assert
            Assert.Equal(states.First(), result.First());
        }

        [Fact]
        public void GetByCountryId_WhenNonExistingCountryIdProvided_ReturnsNull()
        {
            // Arrange
            var states = new List<State> {
                new State {
                    Id = 1,
                    Name = "California",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }},
                new State {
                    Id = 2,
                    Name = "Addis Abeba",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }}
            }.AsQueryable();
            var mockSet = new Mock<DbSet<State>>();
            mockSet.As<IQueryable<State>>().Setup(m => m.Provider).Returns(states.Provider);
            mockSet.As<IQueryable<State>>().Setup(m => m.Expression).Returns(states.Expression);
            mockSet.As<IQueryable<State>>().Setup(m => m.ElementType).Returns(states.ElementType);
            mockSet.As<IQueryable<State>>().Setup(m => m.GetEnumerator()).Returns(states.GetEnumerator());
            _mockContext.Setup(c => c.State).Returns(mockSet.Object);

            // Act
            var result = _mockStateService.GetByCountryId(100);

            // Assert
            Assert.Equal(result.Count(), 0);
        }

        [Fact]
        public void GetById_WhenProvidingExistingStateId_ReturnsState()
        {
            // Arrange
            var states = new List<State> {
                new State {
                    Id = 1,
                    Name = "California",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }},
                new State {
                    Id = 2,
                    Name = "Addis Abeba",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }}
            }.AsQueryable();
            var mockSet = new Mock<DbSet<State>>();
            mockSet.As<IQueryable<State>>().Setup(m => m.Provider).Returns(states.Provider);
            mockSet.As<IQueryable<State>>().Setup(m => m.Expression).Returns(states.Expression);
            mockSet.As<IQueryable<State>>().Setup(m => m.ElementType).Returns(states.ElementType);
            mockSet.As<IQueryable<State>>().Setup(m => m.GetEnumerator()).Returns(states.GetEnumerator());
            _mockContext.Setup(c => c.State.Find(1)).Returns(mockSet.Object.First());
            // Act
            var result = _mockStateService.GetById(1);
            // Assert
            Assert.Equal(states.First(), result);
        }

        [Fact]
        public void GetById_WhenProvidingNonExistingStateId_ReturnsException()
        {
            // Arrange
            var states = new List<State> {
                new State {
                    Id = 1,
                    Name = "California",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }},
                new State {
                    Id = 2,
                    Name = "Addis Abeba",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }}
            }.AsQueryable();
            var mockSet = new Mock<DbSet<State>>();
            mockSet.As<IQueryable<State>>().Setup(m => m.Provider).Returns(states.Provider);
            mockSet.As<IQueryable<State>>().Setup(m => m.Expression).Returns(states.Expression);
            mockSet.As<IQueryable<State>>().Setup(m => m.ElementType).Returns(states.ElementType);
            mockSet.As<IQueryable<State>>().Setup(m => m.GetEnumerator()).Returns(states.GetEnumerator());
            _mockContext.Setup(c => c.State).Returns(mockSet.Object);

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _mockStateService.GetById(3));
            Assert.Equal("State not found", exception.Message);
        }

        [Fact]
        public void GetByName_WhenProvidingExistingStateName_ReturnsFirstState()
        {
            // Arrange
            var states = new List<State> {
                new State {
                    Id = 1,
                    Name = "California",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }},
                new State {
                    Id = 2,
                    Name = "Addis Abeba",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }}
            }.AsQueryable();
            var mockSet = new Mock<DbSet<State>>();
            mockSet.As<IQueryable<State>>().Setup(m => m.Provider).Returns(states.Provider);
            mockSet.As<IQueryable<State>>().Setup(m => m.Expression).Returns(states.Expression);
            mockSet.As<IQueryable<State>>().Setup(m => m.ElementType).Returns(states.ElementType);
            mockSet.As<IQueryable<State>>().Setup(m => m.GetEnumerator()).Returns(states.GetEnumerator());
            _mockContext.Setup(c => c.State).Returns(mockSet.Object);
            // Act
            var result = _mockStateService.GetByName("California", 1);
            // Assert
            Assert.Equal(states.First(), result);
        }

        [Fact]
        public void GetByName_WhenProvidingNonExistingStateName_ReturnsNull()
        {
            // Arrange
            var states = new List<State> {
                new State {
                    Id = 1,
                    Name = "California",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }},
                new State {
                    Id = 2,
                    Name = "Addis Abeba",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }}
            }.AsQueryable();
            var mockSet = new Mock<DbSet<State>>();
            mockSet.As<IQueryable<State>>().Setup(m => m.Provider).Returns(states.Provider);
            mockSet.As<IQueryable<State>>().Setup(m => m.Expression).Returns(states.Expression);
            mockSet.As<IQueryable<State>>().Setup(m => m.ElementType).Returns(states.ElementType);
            mockSet.As<IQueryable<State>>().Setup(m => m.GetEnumerator()).Returns(states.GetEnumerator());
            _mockContext.Setup(c => c.State).Returns(mockSet.Object);
           
            // Act
            var result = _mockStateService.GetByName("North Carolina", 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteState_WhenProvidingExistingStateid_ReturnsVerification()
        {
            // Arrange
            var states = new List<State> {
                new State {
                    Id = 1,
                    Name = "California",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }},
                new State {
                    Id = 2,
                    Name = "Addis Abeba",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }}
            }.AsQueryable();
            var mockSet = new Mock<DbSet<State>>();
            mockSet.As<IQueryable<State>>().Setup(m => m.Provider).Returns(states.Provider);
            mockSet.As<IQueryable<State>>().Setup(m => m.Expression).Returns(states.Expression);
            mockSet.As<IQueryable<State>>().Setup(m => m.ElementType).Returns(states.ElementType);
            mockSet.As<IQueryable<State>>().Setup(m => m.GetEnumerator()).Returns(states.GetEnumerator());
            _mockContext.Setup(c => c.State.Find(1)).Returns(states.First());

            // Act
            _mockStateService.Delete(1);

            //Assert
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);

        }

        [Fact]
        public void DeleteState_WhenProvidingNonExistingStateid_ReturnsException()
        {
            // Arrange
            var states = new List<State> {
                new State {
                    Id = 1,
                    Name = "California",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }},
                new State {
                    Id = 2,
                    Name = "Addis Abeba",
                    CountryId = 1,
                    StateCode="NC",
                    Country= new Country() {
                        Id = 1,
                        Name = "USA",
                        CountryCode = "US"
                    }}
            }.AsQueryable();
            var mockSet = new Mock<DbSet<State>>();
            mockSet.As<IQueryable<State>>().Setup(m => m.Provider).Returns(states.Provider);
            mockSet.As<IQueryable<State>>().Setup(m => m.Expression).Returns(states.Expression);
            mockSet.As<IQueryable<State>>().Setup(m => m.ElementType).Returns(states.ElementType);
            mockSet.As<IQueryable<State>>().Setup(m => m.GetEnumerator()).Returns(states.GetEnumerator());
            _mockContext.Setup(c => c.State.Find(1)).Returns(states.First());

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _mockStateService.Delete(3));
            _mockContext.Verify(m => m.SaveChanges(), Times.Never);
        }
    }
}
