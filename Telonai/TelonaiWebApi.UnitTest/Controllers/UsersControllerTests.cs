
using Amazon.Extensions.CognitoAuthentication;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using TelonaiWebAPI.UnitTest.Helper;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
public class UsersControllerTests
{
    private readonly IFixture _fixture;
    private readonly UsersController _controller;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IInvitationService<InvitationModel, Invitation>> _mockInvitationService;
    private readonly Mock<IEmploymentService<EmploymentModel, Employment>> _mockEmploymentService;
    private readonly Mock<IPersonService<PersonModel, Person>> _mockPersonService;
    private readonly Mock<ITimecardUsaService> _mockTimecardService;
    private readonly Mock<IDayOffRequestService<DayOffRequestModel, DayOffRequest>> _mockDayOffRequestService;

    public UsersControllerTests()
    {
        _fixture = CustomFixture.Create();
        _mockUserService = _fixture.Freeze<Mock<IUserService>>();
        _mockInvitationService = _fixture.Freeze<Mock<IInvitationService<InvitationModel, Invitation>>>();
        _mockEmploymentService = _fixture.Freeze<Mock<IEmploymentService<EmploymentModel, Employment>>>();
        _mockPersonService = _fixture.Freeze<Mock<IPersonService<PersonModel, Person>>>();
        _mockTimecardService = _fixture.Freeze<Mock<ITimecardUsaService>>();
        _mockDayOffRequestService = _fixture.Freeze<Mock<IDayOffRequestService<DayOffRequestModel, DayOffRequest>>>();

        _controller = new UsersController(
            _mockUserService.Object,
            _mockPersonService.Object,
            _mockInvitationService.Object,
            _mockEmploymentService.Object,
            _mockTimecardService.Object,
            _mockDayOffRequestService.Object
        );
    }

    [Theory, CustomAutoData]
    public async Task ForgetPassword_ValidModel_ReturnsNoContent()
    {
        // Arrange
        var username = "testuser";

        // Act
        var result = await _controller.FpRequest(username);

        // Assert
        _mockUserService.Verify(s => s.ForgotPasswordRequest(username), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }
    [Fact]
    public async Task ForgetPassword_InValidModel_ReturnsNoContent()
    {
        // Arrange
        var username = "";
        _controller.ModelState.AddModelError("username", "Username is required.");

        // Act
        var result = await _controller.FpRequest(username);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    [Fact]
    public async Task ForgetPasswordReset_ValidModel_ReturnsNoContent()
    {
        // Arrange
        var username = "birass";
        var passwordResetObject = new PasswordChangeModel()
        {
            Code = "25678",
            Username = "birass",
            NewPassword = "^YHNmju7"
        };
        // Act
        var result = await _controller.FpRequest(passwordResetObject);

        // Assert
        _mockUserService.Verify(s => s.ForgotPasswordResponse(passwordResetObject.Username, passwordResetObject.Code, passwordResetObject.NewPassword), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }
    [Fact]
    public async Task ForgetPasswordReset_InValidModel_ReturnsNoContent()
    {
        // Arrange
        var passwordResetObject = new PasswordChangeModel()
        {
            Code = null,
            Username = null,
            NewPassword = null
        };

        _controller.ModelState.AddModelError("username", "Username is required.");
        _controller.ModelState.AddModelError("code", "Code is required.");
        _controller.ModelState.AddModelError("newPassword", "NewPassword is required.");
        // Act
        var result = await _controller.FpRequest(passwordResetObject);

        // Assert
        _mockUserService.Verify(s => s.ForgotPasswordResponse(passwordResetObject.Username, passwordResetObject.Code, passwordResetObject.NewPassword), Times.Never);
        Assert.IsType<NoContentResult>(result);
    }
    [Fact]
    public async Task ConfirmTwoFactorCodeAsync_ValidModel_ReturnsOkResult()
    {
        var tfa = new TwoFactoreModel()
        {
            Password = "^YHNmju7",
            Username = "birass",
            RememberMachine = true,
            RememberMe = true,
            TwoFactorCode = "2356"
        };
        var signResult = SignInResult.Success;
        _mockUserService.Setup(x => x.ConfirmTwoFactorCodeAsync(tfa)).ReturnsAsync(signResult);

        // Act
        var result = await _controller.ConfirmTwoFactorCodeAsync(tfa);

        // Assert
        _mockUserService.Verify(s => s.ConfirmTwoFactorCodeAsync(tfa), Times.Once);
        Assert.IsType<OkObjectResult>(result);
    }
    [Fact]
    public async Task ConfirmTwoFactorCodeAsync_InValidModel_ReturnsBadRequestResult()
    {
        TwoFactoreModel tfa = null;
        var signResult = SignInResult.NotAllowed;
        _mockUserService.Setup(x => x.ConfirmTwoFactorCodeAsync(tfa)).ReturnsAsync(signResult);

        // Act
        var result = await _controller.ConfirmTwoFactorCodeAsync(tfa);

        // Assert
        _mockUserService.Verify(s => s.ConfirmTwoFactorCodeAsync(tfa), Times.Once);
        Assert.IsType<BadRequestResult>(result);
    }
    [Fact]
    public async Task ConfirmAccountAsync_ValidModel_ReturnsOkResult()
    {
        //assert
        string username = "biras";
        string code = "8347";
        _mockUserService.Setup(x => x.ConfirmAccount(username,code)).ReturnsAsync("Success");
            
        //Act
        var result = await _controller.ConfirmAccountAsync(username,code);

        // Assert
        _mockUserService.Verify(s => s.ConfirmAccount(username,code), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Success", okResult.Value.ToString());
    }
    [Fact]
    public async Task ConfirmAccountAsync_ValidModel_ReturnsOkRequestResult()
    {
        //assert
        string username = "biras";
        string code = "8347";
        _mockUserService.Setup(x => x.ConfirmAccount(username, code)).ReturnsAsync("NewCodeSent");

        //Act
        var result = await _controller.ConfirmAccountAsync(username, code);

        // Assert
        _mockUserService.Verify(s => s.ConfirmAccount(username, code), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("NewCodeSent", okResult.Value.ToString());
    }
    [Fact]
    public async Task ConfirmAccountAsync_ValidModelNewCodeSent_ReturnsOkRequestResult()
    {
        //assert
        string username = "biras";
        string code = "8347";
        _mockUserService.Setup(x => x.ConfirmAccount(username, code)).ReturnsAsync("NewCodeSent");

        //Act
        var result = await _controller.ConfirmAccountAsync(username, code);

        // Assert
        _mockUserService.Verify(s => s.ConfirmAccount(username, code), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("NewCodeSent", okResult.Value.ToString());
    }
    [Fact]
    public async Task ConfirmAccountAsync_WhenInvalidModelPassed_ReturnsBadRequestResult()
    {
        //assert
        string username = null;
        string code = null;
        _mockUserService.Setup(x => x.ConfirmAccount(username, code)).ReturnsAsync("");
        _controller.ModelState.AddModelError("username", "The username parameter is required.");

        //Act
        var result = await _controller.ConfirmAccountAsync(username, code);

        // Assert
        _mockUserService.Verify(s => s.ConfirmAccount(username, code), Times.Never);
        Assert.IsType<BadRequestResult>(result);
    }
    [Fact]
    public async Task CheckUserNameAvailability_WhenValidModelPassed_ReturnOkRequestResult()
    {
        //Assert
        string username = "birass";
        _mockUserService.Setup(x => x.CheckUsernameAvailability(username)).ReturnsAsync(false);

        //Act
        var result = await _controller.CheckUserNameAvailability(username);

        // Assert
        _mockUserService.Verify(s => s.CheckUsernameAvailability(username), Times.Once);
        Assert.IsType<OkResult>(result);
    }
    [Fact]
    public async Task CheckUserNameAvailability_WhenUserNameExists_ReturnBadRequestResult()
    {
        //Assert
        string username = "birass";
        _mockUserService.Setup(x => x.CheckUsernameAvailability(username)).ReturnsAsync(true);

        //Act
        var result = await _controller.CheckUserNameAvailability(username);

        // Assert
        _mockUserService.Verify(s => s.CheckUsernameAvailability(username), Times.Once);
        var response =  Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid", response.Value.ToString());
    }
    [Fact]
    public async Task CheckUserNameAvailability_WhenInValidModelPassed_ReturnOkRequestResult()
    {
        //Assert
        string username = "";
        _mockUserService.Setup(x => x.CheckUsernameAvailability(username)).ReturnsAsync(false);

        //Act
        var result = await _controller.CheckUserNameAvailability(username);

        // Assert
        _mockUserService.Verify(s => s.CheckUsernameAvailability(username), Times.Never);
        var objResponse = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Please provide the username to check whether it is available or not", objResponse.Value.ToString());
    }
    [Fact]
    public async Task ForgetUsername_WhenValidEmailPassed_ReturnOkResult()
    {
        //Assert
        string username = "biras7070@gmail.com";
        _mockUserService.Setup(x => x.SendForgettenUsername(username)).ReturnsAsync(true);

        //Act
        var result = await _controller.ForgetUsername(username);

        // Assert
        _mockUserService.Verify(s => s.SendForgettenUsername(username), Times.Once);
        var objResponse = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Your username was delivered to your email address.Check your email, please.", objResponse.Value.ToString());
    }
    [Fact]
    public async Task ForgetUsername_WhenPassedNonExistingEmail_ReturnNotFoundResult()
    {
        //Assert
        string username = "biras7070@gmail.com";
        _mockUserService.Setup(x => x.SendForgettenUsername(username)).ReturnsAsync(false);

        //Act
        var result = await _controller.ForgetUsername(username);

        // Assert
        _mockUserService.Verify(s => s.SendForgettenUsername(username), Times.Once);
        Assert.IsType<NotFoundResult>(result);
    }
    [Fact]
    public async Task ForgetUsername_WhenInvalidEmailAddressPassed_ReturnException()
    {
        //Assert
        string username = "biras7070gmail.com";
        _mockUserService.Setup(x => x.SendForgettenUsername(username)).ReturnsAsync(false);

        //Act and Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() => _controller.ForgetUsername(username));
        Assert.Equal("Please enter a valid email address.", exception.Message);
        _mockUserService.Verify(s => s.SendForgettenUsername(username), Times.Never);
    }
    [Fact]
    public async Task ForgetUsername_WhenEmptyEmailAddressPassed_ReturnException()
    {
        //Assert
        string username = string.Empty;
        _mockUserService.Setup(x => x.SendForgettenUsername(username)).ReturnsAsync(false);

        //Act and Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() => _controller.ForgetUsername(username));
        Assert.Equal("Please enter email address.", exception.Message);
        _mockUserService.Verify(s => s.SendForgettenUsername(username), Times.Never);
    }
    [Fact]
    public async Task ChangePassword_WhenPassedValidChangePassword_ReturnsOKResult()
    {
        //Arrange
        var Username = "birass";
        var NewPassword = "!QAZxdr5";
        var OldPassword = "^YHNmju7";
        var changePassword = new UserChangePasswordModel()
        {
            Username = Username,
            NewPassword = NewPassword,
            OldPassword = OldPassword
        };

        var signResult = SignInResult.Success;
        _mockUserService.Setup(x => x.ChangePasswordAsync(Username,OldPassword,NewPassword));

        //Act
        var result = await _controller.ChangePassword(changePassword);

        //Assert 
        _mockUserService.Verify(s => s.ChangePasswordAsync(Username, OldPassword, NewPassword), Times.Once);
        Assert.IsType<OkResult>(result);
    }
    [Fact]
    public async Task ChangePassword_WhenPassedInValidChangePassword_ReturnsOKResult()
    {
        //Arrange
        var Username = "birass";
        var NewPassword = "!QAZxdr5";
        var OldPassword = "^YHNmju7";
        UserChangePasswordModel changePassword = null;

        var signResult = SignInResult.Success;
        _mockUserService.Setup(x => x.ChangePasswordAsync(Username, OldPassword, NewPassword));
        _controller.ModelState.AddModelError("change password", "There is no values for changing password");

        //Act
        var result = await _controller.ChangePassword(changePassword);

        //Assert 
        _mockUserService.Verify(s => s.ChangePasswordAsync(Username, OldPassword, NewPassword), Times.Never);
        Assert.IsType<OkResult>(result);
    }
    [Fact]
    public async Task Login_WithSuccessfulAuthentication_ReturnsOkResult()
    {
        //Arrange
        var userInfo = new BaseUser()
        {
            Username = "birass",
            Password = "^YHNmju",
            RememberMe = true
        };
        var signResult =  new Tuple<CognitoUser, SignInManagerResponse>(null, SignInManagerResponse.LoginSucceeded);
        _mockUserService.Setup(x=>x.LoginAsync(userInfo.Username,userInfo.Password,true)).ReturnsAsync(signResult);

        //Act
        var result = await _controller.Login(userInfo);

        //Assert
        Assert.IsType<OkObjectResult>(result);
    }
    [Fact]
    public async Task Login_WithRequiresTwoFactorSuccessfulAuthentication_ReturnsOkResult()
    {
        //Arrange
        var userInfo = new BaseUser()
        {
            Username = "birass",
            Password = "^YHNmju",
            RememberMe = true
        };
        
        var signResult = new Tuple<CognitoUser, SignInManagerResponse>(null, SignInManagerResponse.RequiresTwoFactor);
        _mockUserService.Setup(x => x.LoginAsync(userInfo.Username, userInfo.Password, true)).ReturnsAsync(signResult);

        //Act
        var result = await _controller.Login(userInfo);

        //Assert
        var response = Assert.IsType<OkObjectResult>(result);
        var loginResult = (LoginResult)response.Value;
        Assert.Equal("RequiresTwoFactor", loginResult.Error.ToString());
    }
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsOkResult()
    {
        //Arrange
        var userInfo = new BaseUser()
        {
            Username = "birass",
            Password = "^YHNmju",
            RememberMe = true
        };

        var signResult = new Tuple<CognitoUser, SignInManagerResponse>(null, SignInManagerResponse.InvalidCredentials);
        _mockUserService.Setup(x => x.LoginAsync(userInfo.Username, userInfo.Password, true)).ReturnsAsync(signResult);

        //Act
        var result = await _controller.Login(userInfo);

        //Assert
        var response = Assert.IsType<OkObjectResult>(result);
        var loginResult = (LoginResult)response.Value;
        Assert.Equal("InvalidCredentials", loginResult.Error.ToString());
    }

    [Theory, CustomAutoData]
    public async Task SignUp_ValidModel_ReturnsOk(User user)
    {
      
        var invitation = _fixture.Build<Invitation>()
            .With(i => i.Job, new Job { CompanyId = 1, Id = 1 })
            .With(i => i.ExpirationDate, DateTime.UtcNow.AddDays(1))
            .Create();

        _mockInvitationService.Setup(s => s.GetAllByActivationCodeAndInviteeEmail2(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(invitation);

        _mockUserService.Setup(s => s.SignUpAsync(It.IsAny<User>(), It.IsAny<UserRole>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(IdentityResult.Success);

        var person = _fixture.Create<PersonModel>();
        var employment = _fixture.Create<EmploymentModel>();

        _mockPersonService.Setup(s => s.CreateAsync(It.IsAny<Person>())).ReturnsAsync(person);
        _mockEmploymentService.Setup(s => s.CreateAsync(It.IsAny<Employment>(), It.IsAny<int>())).ReturnsAsync(employment);

        var createProfileFromUserAsync = _controller.GetType().GetMethod("CreateProfileFromUserAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var createEmployment = _controller.GetType().GetMethod("CreateEmploymment", BindingFlags.NonPublic | BindingFlags.Instance);

        await InvokePrivateMethodAsync(createProfileFromUserAsync, _controller, user, invitation.Job.CompanyId);
        await InvokePrivateMethodAsync(createEmployment, _controller, invitation.JobId.Value, person.Id, invitation.Job.CompanyId, (int)SignUpStatusTypeModel.UserProfileCreationStarted);

        var result = await _controller.SignUp(user) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        var loginResult = result.Value as LoginResult;
        Assert.NotNull(loginResult);
        Assert.Single(loginResult.Employments);
        Assert.Equal($"{person.FirstName} {person.LastName}", loginResult.FullName);

        _mockInvitationService.Verify(s => s.UpdateAsync(It.Is<Invitation>(i => i.ExpirationDate <= DateTime.UtcNow)), Times.Once);
    }
    [Theory, CustomAutoData]
    public async Task SignUp_InvalidModel_ReturnsBadRequest(User user)
    { 
        _controller.ModelState.AddModelError("Error", "Model is invalid");
        var result = await _controller.SignUp(user) as BadRequestResult;

        Assert.NotNull(result); 
        Assert.Equal(400, result.StatusCode);
    }
    [Theory, CustomAutoData] 
    public async Task SignUp_SignUpFails_ReturnsBadRequest(User user) 
    { 
      var invitation = _fixture.Build<Invitation>() 
            .With(i => i.Job, new Job { CompanyId = 1, Id = 1 }) 
            .With(i => i.ExpirationDate, DateTime.UtcNow.AddDays(1)) .Create(); 
        _mockInvitationService.Setup(s => s.GetAllByActivationCodeAndInviteeEmail2(It.IsAny<string>(), It.IsAny<string>())) .Returns(invitation); 
        _mockUserService.Setup(s => s.SignUpAsync(It.IsAny<User>(), It.IsAny<UserRole>(), It.IsAny<int>(), It.IsAny<int>())) .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "SignUp Failed" })); 
        var result = await _controller.SignUp(user) as BadRequestResult; 
        Assert.NotNull(result); 
        Assert.Equal(400, result.StatusCode); 
    }
    [Theory, CustomAutoData]
    public async Task SignUp_ExpiredInvitation_ReturnsBadRequest(User user)
    { 
        var invitation = _fixture.Build<Invitation>() 
            .With(i => i.Job, new Job { CompanyId = 1, Id = 1 }) 
            .With(i => i.ExpirationDate, DateTime.UtcNow.AddDays(-1)) 
            .Create(); 
        _mockInvitationService.Setup(s => s.GetAllByActivationCodeAndInviteeEmail2(It.IsAny<string>(), It.IsAny<string>())) .Returns(invitation); 

        var result = await _controller.SignUp(user); 

        var badRequestResult = Assert.IsType<BadRequestResult>(result); 
        Assert.Equal(400, badRequestResult.StatusCode); 
    } 
    [Theory, CustomAutoData] 
    public async Task CreateProfileFromUserAsync_ValidUser_ReturnsPersonModel(User user) 
    { 
        var companyId = 1; 
        var person = _fixture.Create<PersonModel>(); 
        _mockPersonService.Setup(s => s.CreateAsync(It.IsAny<Person>())).ReturnsAsync(person);

        var method = _controller.GetType().GetMethod("CreateProfileFromUserAsync", BindingFlags.NonPublic | BindingFlags.Instance); 

        var result = await InvokePrivateMethodAsync<PersonModel>(method, _controller, user, companyId); 

        Assert.NotNull(result); 
        Assert.Equal(person.FirstName, result.FirstName); 
        Assert.Equal(person.LastName, result.LastName); 
    }
    [Theory, CustomAutoData] public async Task CreateEmploymment_ValidInputs_ReturnsEmploymentModel() {

        var jobId = 1; 
        var personId = 1; 
        var companyId = 1; 
        var statusId = (int)SignUpStatusTypeModel.UserProfileCreationStarted; 
        var employment = _fixture.Create<EmploymentModel>(); 
        _mockEmploymentService.Setup(s => s.CreateAsync(It.IsAny<Employment>(), It.IsAny<int>())).ReturnsAsync(employment); 

        var method = _controller.GetType().GetMethod("CreateEmploymment", BindingFlags.NonPublic | BindingFlags.Instance); 
        var result = await InvokePrivateMethodAsync<EmploymentModel>(method, _controller, jobId, personId, companyId, statusId); 
       
        Assert.NotNull(result); 
        Assert.Equal(employment.JobId, result.JobId); 
        Assert.Equal(employment.PersonId, result.PersonId); 
    }
    private async Task InvokePrivateMethodAsync(MethodInfo method, object obj, params object[] parameters)
    {
        var result = method.Invoke(obj, parameters);
        if (result is Task task)
        { 
            await task;
        }
    }
    private async Task<T> InvokePrivateMethodAsync<T>(MethodInfo method, object obj, params object[] parameters) 
    { 
        var result = method.Invoke(obj, parameters); 
        if (result is Task<T> task) 
        { 
            return await task;
        } 
        throw new InvalidOperationException("Method did not return a Task<T>"); }



}
