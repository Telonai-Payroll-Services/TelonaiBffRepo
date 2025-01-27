using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Controllers
{
    public class UserControllerTest
    {
        private readonly Mock<IInvitationService<InvitationModel, Invitation>> _invitationService;
        private readonly Mock<IUserService> _userService;
        private readonly Mock<IEmploymentService<EmploymentModel, Employment>> _employmentService;
        private readonly Mock<IPersonService<PersonModel, Person>> _personService;
        private readonly Mock<ITimecardUsaService> _timecardService;
        private readonly Mock<DayOffRequestService> _dayOffRequestService;
        private readonly UsersController _usersController;

        public UserControllerTest()
        {
            _employmentService = new Mock<IEmploymentService<EmploymentModel, Employment>>();
            _userService = new Mock<IUserService>();
            _invitationService = new Mock<IInvitationService<InvitationModel, Invitation>>();
            _personService = new Mock<IPersonService<PersonModel, Person>>();
            _timecardService = new Mock<ITimecardUsaService>();
            _dayOffRequestService = new Mock<DayOffRequestService>();   
            _usersController = new UsersController(_userService.Object, _personService.Object, _invitationService.Object, _employmentService.Object, _timecardService.Object,_dayOffRequestService.Object);
        }

        [Fact]
        public async void LoginUser_ProvidingCorrectUsernameAndPassword_ReturnOk()
        {
            //Arrange
            BaseUser user = new BaseUser();
            user.Username = "birass";
            user.Password = "Biras@739313";
            user.RememberMe = true;
            // Setup the CognitoUser and SignInManagerResponse mocks
            var mockCognitoUser = new Mock<CognitoUser>();
            mockCognitoUser.Setup(x => x.Username).Returns("mockUsername");

            // Create the Tuple
            var mockTuple = Tuple.Create(mockCognitoUser.Object, SignInManagerResponse.LoginSucceeded);
         
            _userService.Setup(x => x.LoginAsync(user.Username, user.Password, true)).ReturnsAsync(mockTuple);
           
            //Act
            var result = await _usersController.Login(user);

            //Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
