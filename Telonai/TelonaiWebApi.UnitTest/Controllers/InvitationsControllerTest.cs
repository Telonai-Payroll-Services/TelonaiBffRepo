using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class InvitationsControllerTests
{
    private readonly Mock<IInvitationService<InvitationModel, Invitation>> _serviceMock;
    private readonly Mock<ILogger<InvitationsController>> _loggerMock;
    private readonly Mock<IScopedAuthorization> _scopedAuthorizationMock;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpRequest> _mockHttpRequest;
    private readonly Mock<IEmployerSubscriptionService> _employerSubscriptionServiceMock;
    private readonly InvitationsController _controller;

    public InvitationsControllerTests()
    {
        _serviceMock = new Mock<IInvitationService<InvitationModel, Invitation>>();
        _loggerMock = new Mock<ILogger<InvitationsController>>();
        _scopedAuthorizationMock = new Mock<IScopedAuthorization>();
        _employerSubscriptionServiceMock = new Mock<IEmployerSubscriptionService>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpRequest = new Mock<HttpRequest>();
        _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
        _controller = new InvitationsController(_serviceMock.Object, _loggerMock.Object, _scopedAuthorizationMock.Object, _employerSubscriptionServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            }
        }; ;
    }
    [Fact]
    public void GetByActivationCodeAndEmail_ValidData_ReturnsOkResult()
    {
        var model = new InvitationRequestModel { code = "ABCDEFGH", Email = "test@example.com" };
        var invitationModel = new InvitationModel();
        _serviceMock.Setup(s => s.GetAllByActivaionCodeAndInviteeEmail(model.code, model.Email)).Returns(invitationModel);

        var result = _controller.GetByActivationCodeAndEmail(model);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(invitationModel, okResult.Value);
    }


[Fact]
public void GetByActivationCodeAndEmail_InvalidActivationCode_ThrowsException()
    {
        var model = new InvitationRequestModel { code = "ABCDE", Email = "test@example.com" };
        Assert.Throws<InvalidDataException>(() => _controller.GetByActivationCodeAndEmail(model));
    }

    [Fact]
    public void GetByActivationCodeAndEmail_InvalidEmail_ThrowsException()
    {
        var model = new InvitationRequestModel { code = "ABCDEFGH", Email = "invalid-email" };
        Assert.Throws<InvalidDataException>(() => _controller.GetByActivationCodeAndEmail(model));
    }
    [Fact]
    public async Task InviteEmployee_ValidData_ReturnsOkResult()
    {
        var model = new InvitationModel
        {
            Email = "test@example.com",
            Employment = new EmploymentModel { CompanyId = 1 }
        };

        var currentUserEmail = model.Email;
        var claims = new List<Claim>
        {
            new Claim("email", currentUserEmail),
            new Claim("custom:scope", "Admin")
             };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(c => c.User).Returns(principal);
        _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
        _scopedAuthorizationMock.Setup(s => s.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, model.Employment.CompanyId));

        var result = _controller.InviteEmployee(model);

        var okResult = Assert.IsType<OkResult>(result);
        _scopedAuthorizationMock.Verify(s => s.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, model.Employment.CompanyId), Times.Once);
        _serviceMock.Verify(s => s.CreateAsync(model, true), Times.Once);
    }
    [Fact]
    public void InviteEmployee_UnauthorizedUser_ThrowsException()
    {
        var model = new InvitationModel
        {
            Email = "test@example.com",
            Employment = new EmploymentModel { CompanyId = 1 }
        };

        var currentUserEmail = model.Email;
        var claims = new List<Claim>
        {
            new Claim("email", currentUserEmail),
            new Claim("custom:scope", "Admin")
             };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(c => c.User).Returns(principal);
        _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
        _scopedAuthorizationMock.Setup(s => s.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, model.Employment.CompanyId))
                                 .Throws(new UnauthorizedAccessException());

        Assert.Throws<UnauthorizedAccessException>(() => _controller.InviteEmployee(model));
        _serviceMock.Verify(s => s.CreateAsync(It.IsAny<InvitationModel>(), It.IsAny<bool>()), Times.Never);
    }
    [Fact]
    public async Task InviteEmployer_ValidData_ReturnsOkResult()
    {
        var model = new EmployerInvitationModel
        {
            Email = "test@example.com",
            Company = "Test Company",
            AccountNumber = 123456,
            AccountNumber2 = 654321,
            RoutingNumber = 111000,
            AccountType = BankAccountTypeModel.Saving,
            City = "Test City",
            State = "TS",
            Zip = 12345,
            FirstName = "John",
            LastName = "Doe",
            TaxId = "12345",
            NumberOfEmployees = 100,
            SubscriptionType = SubscriptionTypeModel.Annually
        };
        var code = "1234"; 
        var agentCode = ushort.Parse(code); 
        var invitation = new Invitation { Id = Guid.NewGuid() }; 
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<InvitationModel>(), false)).ReturnsAsync(invitation);
        _employerSubscriptionServiceMock.Setup(e => e.CreateAsync(It.IsAny<EmployerSubscriptionModel>())).Returns(Task.CompletedTask); 

        var result = await _controller.InviteEmployer(model, code);

        var okResult = Assert.IsType<OkResult>(result); 
        _serviceMock.Verify(s => s.CreateAsync(It.Is<InvitationModel>(im => im.Email == model.Email && im.Company == model.Company && im.FirstName == model.FirstName && im.LastName == model.LastName && im.TaxId == model.TaxId ), false), Times.Once); 
        _employerSubscriptionServiceMock.Verify(e => e.CreateAsync(It.IsAny<EmployerSubscriptionModel>()), Times.Once);
    }
    [Fact]
    public async Task InviteEmployer_InvalidAgentCode_ReturnsOkResultWithMessage()
    {
        var model = new EmployerInvitationModel
        {
            Email = "test@example.com",
            Company = "Test Company",
            AccountNumber = 123456,
            AccountNumber2 = 654321,
            RoutingNumber = 111000,
            AccountType = BankAccountTypeModel.Saving,
            City = "Test City",
            State = "TS",
            Zip = 12345,
            FirstName = "John",
            LastName = "Doe",
            TaxId = "12345",
            NumberOfEmployees = 100,
            SubscriptionType = SubscriptionTypeModel.Annually
        };
        var code = "abcd"; 

        var result = await _controller.InviteEmployer(model, code);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = okResult.Value as string;
        Assert.Contains("Invalid Agent Code", message);
        _serviceMock.Verify(s => s.CreateAsync(It.IsAny<InvitationModel>(), It.IsAny<bool>()), Times.Never);
        _employerSubscriptionServiceMock.Verify(e => e.CreateAsync(It.IsAny<EmployerSubscriptionModel>()), Times.Never);
    }


}
