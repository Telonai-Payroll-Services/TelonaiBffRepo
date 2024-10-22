using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

public class PayrollScheduleControllerTests
{
    private readonly Mock<IPayrollScheduleService> _mockPayrollScheduleService;
    private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
    private readonly PayrollScheduleController _controller;

    public PayrollScheduleControllerTests()
    {
        _mockPayrollScheduleService = new Mock<IPayrollScheduleService>();
        _mockScopedAuthorization = new Mock<IScopedAuthorization>();
        _controller = new PayrollScheduleController(_mockPayrollScheduleService.Object, _mockScopedAuthorization.Object);
    }

    [Fact]
    public void GetPayrollSchedule_ReturnsOkResult_WithPayrollSchedule()
    {

        int companyId = 1;
        var mockPayrollSchedule = new PayrollScheduleModel { CompanyId = 1, StartDate = DateTime.UtcNow };
        _mockPayrollScheduleService.Setup(service => service.GetLatestByCompanyId(companyId)).Returns(mockPayrollSchedule);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetPayrollSchedule(companyId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PayrollScheduleModel>(okResult.Value);
        Assert.Equal(mockPayrollSchedule, returnValue);
        _mockScopedAuthorization.Verify();
    }

    [Fact]
    public void GetPayrollSchedule_ReturnsUnauthorized_WhenAuthorizationFails()
    {

        int companyId = 1;
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId))
            .Throws(new UnauthorizedAccessException());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        Assert.Throws<UnauthorizedAccessException>(() => _controller.GetPayrollSchedule(companyId));
    }

    [Fact]
    public void GetPayrollSchedule_ReturnsNull_WhenPayrollScheduleNotFound()
    {
        int companyId = 1;
        _mockPayrollScheduleService.Setup(service => service.GetLatestByCompanyId(companyId)).Returns((PayrollScheduleModel)null);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetPayrollSchedule(companyId);

        var notFoundResult = Assert.IsType<OkObjectResult>(result);
        Assert.Null(notFoundResult.Value);
        _mockScopedAuthorization.Verify();
    }
    [Fact]
    public void GetAllPayrollSchedules_ReturnsOkResult_WithPayrollSchedules()
    {
        int companyId = 1;
        var mockPayrollSchedules = new List<PayrollScheduleModel> {
            new PayrollScheduleModel { CompanyId = 1, StartDate = DateTime.UtcNow }
        };
        _mockPayrollScheduleService.Setup(service => service.GetByCompanyId(companyId)).Returns(mockPayrollSchedules);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetAllPayrollSchedules(companyId);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<PayrollScheduleModel>>(okResult.Value);
        Assert.Equal(mockPayrollSchedules, returnValue);
        _mockScopedAuthorization.Verify();
    }

    [Fact]
    public void GetAllPayrollSchedules_ReturnsUnauthorized_WhenAuthorizationFails()
    {
        int companyId = 1;
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId))
            .Throws(new UnauthorizedAccessException());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        Assert.Throws<UnauthorizedAccessException>(() => _controller.GetAllPayrollSchedules(companyId));


    }

    [Fact]
    public void GetAllPayrollSchedules_ReturnsNull_WhenPayrollSchedulesNotFound()
    {
        int companyId = 1;
        _mockPayrollScheduleService.Setup(service => service.GetByCompanyId(companyId)).Returns((List<PayrollScheduleModel>)null);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetAllPayrollSchedules(companyId);

        var notFoundResult = Assert.IsType<OkObjectResult>(result);
        Assert.Null(notFoundResult.Value);
        _mockScopedAuthorization.Verify();
    }

    [Fact]
    public void GetById_ReturnsOkResult_WithPayrollSchedule()
    {

        int id = 1;
        var mockPayrollSchedule = new PayrollScheduleModel { Id = 1, CompanyId = 1, StartDate = DateTime.UtcNow };
        _mockPayrollScheduleService.Setup(service => service.GetById(id)).Returns(mockPayrollSchedule);

        var result = _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PayrollScheduleModel>(okResult.Value);
        Assert.Equal(mockPayrollSchedule, returnValue);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenPayrollScheduleNotFound()
    {

        int id = 1;

        var result = _controller.GetById(id);

        var notFoundResult = Assert.IsType<OkObjectResult>(result);
        Assert.Null(notFoundResult.Value);
        _mockScopedAuthorization.Verify();
    }

    [Fact]
    public void Create_ReturnsOkResult_WithMessage()
    {
        var model = new PayrollScheduleModel { CompanyId = 1, };
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, model.CompanyId)).Verifiable();
        _mockPayrollScheduleService.Setup(service => service.Create(model)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.Create(model);

        var retuenMessage = new { message = "Payroll Schedule created." };
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(retuenMessage.ToString(), okResult.Value.ToString());
        _mockScopedAuthorization.Verify();

    }
    [Fact]
    public void Create_ReturnsUnauthorized_WhenAuthorizationFails()
    {
        var model = new PayrollScheduleModel { CompanyId = 1, };
        _mockPayrollScheduleService.Setup(service => service.Create(model))
         .Throws(new UnauthorizedAccessException());
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
       {
            new Claim(ClaimTypes.NameIdentifier, "1"),
       }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;
        Assert.Throws<UnauthorizedAccessException>(() => _controller.Create(model));

    }
    [Fact]
    public void Update_ReturnsOkResult_WithMessage()
    {
        int id = 1;
        var model = new PayrollScheduleModel { CompanyId = 1, };
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, model.CompanyId)).Verifiable();
        _mockPayrollScheduleService.Setup(service => service.Update(id, model)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.Update(id, model);
        var message = new { message = "Payroll Schedule updated." };
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(message.ToString(), okResult.Value.ToString());
        _mockScopedAuthorization.Verify();
        _mockPayrollScheduleService.Verify();
    }

    [Fact]
    public void Update_ReturnsUnauthorized_WhenAuthorizationFails()
    {

        int id = 1;
        var model = new PayrollScheduleModel { CompanyId = 1, };
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, model.CompanyId))
            .Throws(new UnauthorizedAccessException());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;
        Assert.Throws<UnauthorizedAccessException>(() => _controller.Update(id, model));
    }
    [Fact]
    public void Delete_ReturnsOkResult_WithMessage()
    {
        int id = 1;
        _mockPayrollScheduleService.Setup(service => service.Delete(id)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.Delete(id);
        var message = new { message = "Payroll Schedule deleted." };
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(message.ToString(), okResult.Value.ToString());

        _mockPayrollScheduleService.Verify();
    }

    [Fact]
    public void Delete_ReturnsUnauthorized_WhenAuthorizationFails()
    {
        int id = 1;
        _mockPayrollScheduleService.Setup(service => service.Delete(id))
            .Throws(new UnauthorizedAccessException());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        Assert.Throws<UnauthorizedAccessException>(() => _controller.Delete(id));
    }
}
