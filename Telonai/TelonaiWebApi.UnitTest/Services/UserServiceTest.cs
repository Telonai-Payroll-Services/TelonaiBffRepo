using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Services
{
    public class UserServiceTest
    {
        private readonly Mock<SignInManager<CognitoUser>> _mockSignInManager;
        private readonly Mock<CognitoSignInManager<CognitoUser>> _mockSignInManager2;
        private readonly Mock<CognitoUserManager<CognitoUser>> _mockUserManager;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly Mock<CognitoUserPool> _mockPool;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly UserService _userService;

        public UserServiceTest()
        {
            _mockSignInManager = new Mock<SignInManager<CognitoUser>>();    
            _mockSignInManager2 = new Mock<CognitoSignInManager<CognitoUser>>();
            _mockUserManager = new Mock<CognitoUserManager<CognitoUser>>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockPool = new Mock<CognitoUserPool>();
            _httpContextAccessor =  new Mock<IHttpContextAccessor>();
            _userService = new UserService(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object, _mockPool.Object, _httpContextAccessor.Object);
        }
        [Fact]
        public async void LoginAsync_WhenProvidningCorrectUsernameAndPassword_ReturnUserInfo()
        {
            //Arrange 
            _mockSignInManager.Setup(x => x.PasswordSignInAsync("birass", "Biras@739313", true, true)).ReturnsAsync(It.IsAny<SignInResult>);

            //Act
            var result = await _userService.LoginAsync("birass", "Biras@739313", true);
            
            //Assert
            //Assert.is
        }

    }
}
