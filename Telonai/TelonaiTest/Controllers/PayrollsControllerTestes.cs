using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TelonaiWebApi.Models;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.UnitTest.Controllers;
public class PayrollsControllerTests
{
    private readonly Mock<IPayrollService> _mockPayrollService;
    private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpRequest> _mockHttpRequest;
    private readonly PayrollsController _controller;

    public PayrollsControllerTests()
    {
        _mockPayrollService = new Mock<IPayrollService>();
        _mockScopedAuthorization = new Mock<IScopedAuthorization>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpRequest = new Mock<HttpRequest>();
        _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
        _controller = new PayrollsController(_mockPayrollService.Object, _mockScopedAuthorization.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            }
    };
    }

    [Fact]
    public void GetCurrentPayroll_ReturnsOkResult_WithPayroll()
    {
        
        int companyId = 1;
        var mockPayroll = new PayrollModel { Id=1,CompanyId=23,TrueRunDate=DateTime.Now };
        _mockPayrollService.Setup(service => service.GetCurrentPayroll(companyId)).Returns(mockPayroll);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
        var scopeClaim = new Claim("custom:scope", "SystemAdmin");
        var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
        var principal = new ClaimsPrincipal(claimsIdentity);

        _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
      
        var result = _controller.GetCurrentPayroll(companyId);
       
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PayrollModel>(okResult.Value);
        Assert.Equal(mockPayroll, returnValue);
        _mockScopedAuthorization.Verify();
    }
    [Fact]
    public void GetCurrentPayroll_ReturnsUnauthorized_WhenAuthorizationFails()
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
     
        Assert.Throws<UnauthorizedAccessException>(() => _controller.GetCurrentPayroll(companyId));
       
    }
    [Fact]
    public void GetCurrentPayroll_ReturnsNull_WhenPayrollNotFound()
    {

        int companyId = 1;
        _mockPayrollService.Setup(service => service.GetCurrentPayroll(companyId)).Returns((PayrollModel)null);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetCurrentPayroll(companyId);

        var notFoundResult = Assert.IsType<OkObjectResult>(result);
        _mockScopedAuthorization.Verify();
    }

    [Fact]
    public void GetPreviousPayroll_ReturnsOkResult_WithPayroll()
    {
        int companyId = 1;
        var mockPayroll = new PayrollModel { Id = 1, CompanyId = 25, TrueRunDate = DateTime.UtcNow.AddDays(-10) };
        _mockPayrollService.Setup(service => service.GetPreviousPayroll(companyId)).Returns(mockPayroll);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetPreviousPayroll(companyId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PayrollModel>(okResult.Value);
        Assert.Equal(mockPayroll, returnValue);
        _mockScopedAuthorization.Verify();
    }
    [Fact]
    public void GetPreviousPayroll_ReturnsNotFound_WhenPayrollNotFound()
    {
        int companyId = 1;
        _mockPayrollService.Setup(service => service.GetPreviousPayroll(companyId)).Returns((PayrollModel)null);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetPreviousPayroll(companyId);

        var notFoundResult = Assert.IsType<OkObjectResult>(result);
        _mockScopedAuthorization.Verify();
    }
    [Fact]
    public void GetPreviousPayroll_ReturnsUnauthorized_WhenAuthorizationFails()
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

    
        Assert.Throws<UnauthorizedAccessException>(() => _controller.GetPreviousPayroll(companyId));
    }

    [Fact]
    public void GetByCompanyAndTimeForUser_ReturnsOkResult_WithPayroll()
    {

        int companyId = 1;
        DateOnly startTime = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
        DateOnly endTime = DateOnly.FromDateTime(DateTime.Now);
        var mockPayroll = new List<PayrollModel> { new PayrollModel { Id = 1, CompanyId = 23, TrueRunDate = DateTime.UtcNow.AddDays(-10) } };
        _mockPayrollService.Setup(service => service.GetReport(companyId, startTime, endTime)).Returns(mockPayroll);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetByCompanyAndTimeForUser(companyId, startTime, endTime);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<PayrollModel>>(okResult.Value);
        Assert.Equal(mockPayroll, returnValue);
        _mockScopedAuthorization.Verify();
    }

    [Fact]
    public void GetByCompanyAndTimeForUser_ReturnsNull_WhenPayrollNotFound()
    {

        int companyId = 1;
        DateOnly startTime = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
        DateOnly endTime = DateOnly.FromDateTime(DateTime.Now);
        _mockPayrollService.Setup(service => service.GetReport(companyId, startTime, endTime)).Returns((List<PayrollModel>)null);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetByCompanyAndTimeForUser(companyId, startTime, endTime);

        var notFoundResult = Assert.IsType<OkObjectResult>(result);
        _mockScopedAuthorization.Verify();
    }
    [Fact]
    public void GetByCompanyAndTimeForUser_ReturnsUnauthorized_WhenAuthorizationFails()
    {
        int companyId = 1;
        DateOnly startTime = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
        DateOnly endTime = DateOnly.FromDateTime(DateTime.Now);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId))
            .Throws(new UnauthorizedAccessException());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        Assert.Throws<UnauthorizedAccessException>(() => _controller.GetByCompanyAndTimeForUser(companyId, startTime, endTime));
    }

    [Fact]
    public void GetByCompanyAndCount_ReturnsOkResult_WithPayroll()
    {
 
        int companyId = 1;
        int count = 10;
        var mockPayroll = new List<PayrollModel> { new PayrollModel { Id = 1, CompanyId = 23, TrueRunDate = DateTime.UtcNow.AddDays(-10) } };
        _mockPayrollService.Setup(service => service.GetLatestByCount(companyId, count)).Returns(mockPayroll);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetByCompanyAndCount(companyId, count);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<PayrollModel>>(okResult.Value);
        Assert.Equal(mockPayroll, returnValue);
        _mockScopedAuthorization.Verify();
    }

    [Fact]
    public void GetByCompanyAndCount_ReturnsNull_WhenPayrollNotFound()
    {

        int companyId = 1;
        int count = 10;
        _mockPayrollService.Setup(service => service.GetLatestByCount(companyId, count)).Returns((List<PayrollModel>)null);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetByCompanyAndCount(companyId, count);

         var notFoundResult = Assert.IsType<OkObjectResult>(result);
        _mockScopedAuthorization.Verify();
    }
    [Fact]
    public void GetByCompanyAndCount_ReturnsUnauthorized_WhenAuthorizationFails()
    {
        int companyId = 1;
        int count = 10;
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId))
           .Throws(new UnauthorizedAccessException());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        Assert.Throws<UnauthorizedAccessException>(() => _controller.GetByCompanyAndCount(companyId, count));
    }
    [Fact]
    public void GetById_ReturnsOkResult_WithPayroll()
    {
        int id = 1;
        var mockPayroll = new PayrollModel { Id = id, CompanyId = 1  };
        _mockPayrollService.Setup(service => service.GetById(id)).Returns(mockPayroll);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.User, mockPayroll.CompanyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PayrollModel>(okResult.Value);
        Assert.Equal(mockPayroll, returnValue);
        _mockScopedAuthorization.Verify();
    }

    [Fact]
    public void GetById_ReturnsNull_WhenPayrollNotFound()
    {
        int id = 1;
        int companyId = 3;
        _mockPayrollService.Setup(service => service.GetById(id)).Returns((PayrollModel)null);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.User, companyId)).Verifiable();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.GetById(id);

        var notFoundResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void GetById_ReturnsUnauthorized_WhenAuthorizationFails()
    {
        int id = 1;
        var mockPayroll = new PayrollModel { Id = id, CompanyId = 1  };
        _mockPayrollService.Setup(service => service.GetById(id)).Returns(mockPayroll);
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.User, mockPayroll.CompanyId))
            .Throws(new UnauthorizedAccessException());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        Assert.Throws<UnauthorizedAccessException>(() => _controller.GetById(id));
    }
    [Fact]
    public void Create_ReturnsOkResult_WithMessage()
    {
       
        int companyId = 1;
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();
        _mockPayrollService.Setup(service => service.Create(companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.Create(companyId);

        var message = new { message = "Payroll created." };
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(message.ToString(), okResult.Value.ToString());
        _mockScopedAuthorization.Verify();
        _mockPayrollService.Verify();
    }

    [Fact]
    public void Create_ReturnsUnauthorized_WhenAuthorizationFails()
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

        Assert.Throws<UnauthorizedAccessException>(() => _controller.Create(companyId));
    }
    [Fact]
    public void CreateNextPayrollForAll_ReturnsOkResult_WithMessage()
    {
        int payrollsGenerated = 5;
        _mockPayrollService.Setup(service => service.CreateNextPayrollForAll()).ReturnsAsync(payrollsGenerated);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;
        var retuenMessage = new { message = $"{payrollsGenerated} Payrolls generated." };
        var result = _controller.CreateNextPayrollForAll();

        var okResult = Assert.IsType<OkObjectResult>(result);      
        Assert.Equal(retuenMessage.ToString(), okResult.Value.ToString());
        _mockPayrollService.Verify(service => service.CreateNextPayrollForAll(), Times.Once);
    }
     [Fact]
     public void CreateNextPayrollForAll_ReturnsUnauthorized_WhenAuthorizationFails()
     {
        _mockPayrollService.Setup(service => service.CreateNextPayrollForAll())
         .Throws(new UnauthorizedAccessException());
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
       {
            new Claim(ClaimTypes.NameIdentifier, "1"),
       }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;
        Assert.Throws<UnauthorizedAccessException>(() => _controller.CreateNextPayrollForAll());

    }

    [Fact]
    public void Update_ReturnsOkResult_WithMessage()
    {
        int id = 1;
        int companyId = 1;
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Verifiable();
        _mockPayrollService.Setup(service => service.Update(id, companyId)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.Update(id, companyId);
        var message = new { message = "Payrolls updated." };
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(message.ToString(), okResult.Value.ToString());
        _mockScopedAuthorization.Verify();
        _mockPayrollService.Verify();
    }

    [Fact]
    public void Update_ReturnsUnauthorized_WhenAuthorizationFails()
    {
       
        int id = 1;
        int companyId = 1;
        _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId))
            .Throws(new UnauthorizedAccessException());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;
        Assert.Throws<UnauthorizedAccessException>(() => _controller.Update(id, companyId));
    }

    [Fact]
    public void Delete_ReturnsOkResult_WithMessage()
    {
        int id = 1;
        _mockPayrollService.Setup(service => service.Delete(id)).Verifiable();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        var context = new DefaultHttpContext { User = user };
        _controller.ControllerContext.HttpContext = context;

        var result = _controller.Delete(id);
        var message = new { message = "Payroll deleted." };
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(message.ToString(), okResult.Value.ToString());
     
        _mockPayrollService.Verify();
    }

    [Fact]
    public void Delete_ReturnsUnauthorized_WhenAuthorizationFails()
    {
        int id = 1;
        _mockPayrollService.Setup(service => service.Delete(id))
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
