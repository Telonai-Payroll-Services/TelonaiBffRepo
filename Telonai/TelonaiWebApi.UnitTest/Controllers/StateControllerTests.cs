using AutoMapper;
using Moq;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Controllers
{
    public class StateControllerTests
    {
        private readonly Mock<IStateService> _mockStateService;
        private readonly Mock<IMapper> _mockMapper;
        public StatesController _StatesController;

        public StateControllerTests()
        {
            _mockMapper = new Mock<IMapper>();
            _mockStateService = new Mock<IStateService>();
            _StatesController = new StatesController(_mockStateService.Object, _mockMapper.Object);
        }

        [Fact]
        public void GetByCountryId_WithProvidingExistingCountryId_ReturnsOkResult()
        {
            //Arrange
            var country = new Country
            {
                Id = 1,
                Name = "USA"
            };
            var states = new List<State>
            {
                new State { Id = 1, Name = "State A" },
                new State { Id = 2, Name = "State B" }
            };
            var statesModel = new List<StateModel>
            {
                new StateModel { Id = 1, Name = "State A",Country = "USA", CountryId = 1 },
                new StateModel { Id = 2, Name = "State B" ,Country = "USA", CountryId = 1}
            };
            _mockStateService.Setup(x => x.GetByCountryId(1)).Returns(states);
            _mockMapper.Setup(x => x.Map<IList<StateModel>>(states)).Returns(statesModel);
            //Act
            var result = _StatesController.GetByCountryId(1);
            //Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
            var returnedStates = Assert.IsType<List<StateModel>>(okResult.Value);
            Assert.Equal(statesModel, returnedStates);
        }

        [Fact]
        public void GetByCountryId_WithProvidingNonExistingCountryId_ReturnsOkResult()
        {
            //Arrange
            var country = new Country
            {
                Id = 1,
                Name = "USA"
            };
            var states = new List<State>
            {
                new State { Id = 1, Name = "State A" },
                new State { Id = 2, Name = "State B" }
            };
            var statesModel = new List<StateModel>
            {
                new StateModel { Id = 1, Name = "State A",Country = "USA", CountryId = 1 },
                new StateModel { Id = 2, Name = "State B" ,Country = "USA", CountryId = 1}
            };
            _mockStateService.Setup(x => x.GetByCountryId(1)).Returns(states);
            _mockMapper.Setup(x => x.Map<IList<StateModel>>(states)).Returns((List<StateModel>)null);
            //Act
            var result = _StatesController.GetByCountryId(11);
            //Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public void GetByName_WithProvidingExistingStateName_ReturnsOkResult()
        {
            //Arrange
            var country = new Country
            {
                Id = 1,
                Name = "USA"
            };
            var state = new State
            {
                Id = 1,
                Name = "State A",
                Country = country,
                CountryId = 1
            };
            var stateModel = new StateModel
            {
                Id = 1,
                Name = "State A",
                Country = "USA",
                CountryId = 1
            };
            _mockStateService.Setup(x => x.GetByName("State A", 1)).Returns(state);
            _mockMapper.Setup(x => x.Map<StateModel>(state)).Returns(stateModel);

            //Act
            var result = _StatesController.GetByName("State A", 1);

            //Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
            var returnedState = Assert.IsType<StateModel>(okResult.Value);
            Assert.Equal(stateModel, returnedState);
        }

        [Fact]
        public void GetByName_WithProvidingNonExistingStateName_ReturnsOkResultWithNull()
        {
            //Arrange
            var country = new Country
            {
                Id = 1,
                Name = "USA"
            };
            var state = new State
            {
                Id = 1,
                Name = "State A",
                Country = country,
                CountryId = 1
            };
            var stateModel = new StateModel
            {
                Id = 1,
                Name = "State A",
                Country = "USA",
                CountryId = 1
            };
            _mockStateService.Setup(x => x.GetByName("State A", 1)).Returns(state);
            _mockMapper.Setup(x => x.Map<StateModel>(state)).Returns(stateModel);

            //Act
            var result = _StatesController.GetByName("State B", 1);

            //Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public void GetById_WithProvidingExistingStateName_ReturnsOkResult()
        {
            //Arrange
            var country = new Country
            {
                Id = 1,
                Name = "USA"
            };
            var state = new State
            {
                Id = 1,
                Name = "State A",
                Country = country,
                CountryId = 1
            };
            var stateModel = new StateModel
            {
                Id = 1,
                Name = "State A",
                Country = "USA",
                CountryId = 1
            };
            _mockStateService.Setup(x => x.GetById(1)).Returns(state);
            _mockMapper.Setup(x => x.Map<StateModel>(state)).Returns(stateModel);

            //Act
            var result = _StatesController.GetById(1);

            //Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
            var returnedState = Assert.IsType<StateModel>(okResult.Value);
            Assert.Equal(stateModel, returnedState);
        }

        [Fact]
        public void GetById_WithProvidingExistingStateName_ReturnsOkWithNullResult()
        {
            //Arrange
            var country = new Country
            {
                Id = 1,
                Name = "USA"
            };
            var state = new State
            {
                Id = 1,
                Name = "State A",
                Country = country,
                CountryId = 1
            };
            var stateModel = new StateModel
            {
                Id = 1,
                Name = "State A",
                Country = "USA",
                CountryId = 1
            };
            _mockStateService.Setup(x => x.GetById(11))
                             .Throws(new Exception("State not found"));

            //Act
            var result = _StatesController.GetById(11);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }
    }
}
