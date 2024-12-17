
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Identity;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using TelonaiWebAPI.UnitTest.Helper;

public class UsersControllerTests
{
    private readonly IFixture _fixture;
    private readonly UsersController _controller;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IInvitationService<InvitationModel, Invitation>> _mockInvitationService;
    private readonly Mock<IEmploymentService<EmploymentModel, Employment>> _mockEmploymentService;
    private readonly Mock<IPersonService<PersonModel, Person>> _mockPersonService;
    private readonly Mock<ITimecardUsaService> _mockTimecardService;

    public UsersControllerTests()
    {
        _fixture = CustomFixture.Create();
        _mockUserService = _fixture.Freeze<Mock<IUserService>>();
        _mockInvitationService = _fixture.Freeze<Mock<IInvitationService<InvitationModel, Invitation>>>();
        _mockEmploymentService = _fixture.Freeze<Mock<IEmploymentService<EmploymentModel, Employment>>>();
        _mockPersonService = _fixture.Freeze<Mock<IPersonService<PersonModel, Person>>>();
        _mockTimecardService = _fixture.Freeze<Mock<ITimecardUsaService>>();

        _controller = new UsersController(
            _mockUserService.Object,
            _mockPersonService.Object,
            _mockInvitationService.Object,
            _mockEmploymentService.Object,
            _mockTimecardService.Object
        );
    }

    [Theory, CustomAutoData]
    public async Task SignUp_ValidModel_ReturnsOk(User user)
    {
      
        var invitation = _fixture.Build<Invitation>()
            .With(i => i.Job, new Job { CompanyId = 1, Id = 1 })
            .With(i => i.ExpirationDate, DateTime.UtcNow.AddDays(1))
            .Create();

        _mockInvitationService.Setup(s => s.GetAllByActivaionCodeAndInviteeEmail2(It.IsAny<string>(), It.IsAny<string>()))
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
        _mockInvitationService.Setup(s => s.GetAllByActivaionCodeAndInviteeEmail2(It.IsAny<string>(), It.IsAny<string>())) .Returns(invitation); 
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
        _mockInvitationService.Setup(s => s.GetAllByActivaionCodeAndInviteeEmail2(It.IsAny<string>(), It.IsAny<string>())) .Returns(invitation); 

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
        Assert.Equal(person.LastName, result.LastName); } 
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
