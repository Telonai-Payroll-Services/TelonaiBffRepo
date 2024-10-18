using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.UnitTest.Controllers
{
    public class PersonsControllerTests
    {
        private readonly Mock<IPersonService<PersonModel, Person>> _mockPersonService;
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly PersonsController _controller;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        public PersonsControllerTests()
        {
            _mockPersonService = new Mock<IPersonService<PersonModel, Person>>();
            _mockScopedAuthorization = new Mock<IScopedAuthorization>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _controller = new PersonsController(_mockPersonService.Object, _mockScopedAuthorization.Object)/**/
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }
        [Fact]
        public void GetByCompanyId_ReturnsOkResult_WithListOfPersons()
        {

            var mockHttpContext = new Mock<HttpContext>();

            var samplePersons = new List<PersonModel> { new PersonModel { Id = 1, FirstName = "Person A" }, new PersonModel { Id = 2, FirstName = "Person B" } };
            _mockPersonService.Setup(s => s.GetByCompanyId(It.IsAny<int>())).Returns(samplePersons);


            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "SystemAdmin");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);

            mockHttpContext.Setup(c => c.User).Returns(principal);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            _mockScopedAuthorization.Setup(a => a.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, It.IsAny<int>()));

            var result = _controller.GetByCompanyId(1);


            Assert.IsType<OkObjectResult>(result);
            var actualPersons = (List<PersonModel>)((OkObjectResult)result).Value;
            Assert.Equal(samplePersons.Count, actualPersons.Count);

        }
        [Fact]
        public void GetByCompanyId_ReturnsUnauthorized_WhenNotAuthorized()
        {
            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "User");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);

            _mockScopedAuthorization.Setup(a => a.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, It.IsAny<int>()))
            .Throws(new UnauthorizedAccessException());

            Assert.Throws<UnauthorizedAccessException>(() => _controller.GetByCompanyId(1));

           /* var result = _controller.GetByCompanyId(1);


            Assert.IsType<UnauthorizedResult>(result);*/
        }
        [Fact]
        public void GetAll_ReturnsOkResult_WithListOfProfiles()
        {

            var mockProfiles = new List<PersonModel> { new PersonModel { Id = 1, FullName = "full name" } };
            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "SystemAdmin");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            _mockPersonService.Setup(service => service.Get()).Returns(mockProfiles);
            _mockScopedAuthorization.Setup(auth => auth.Validate(It.IsAny<ClaimsPrincipal>(), AuthorizationType.SystemAdmin)).Verifiable();
      
            var result = _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<PersonModel>>(okResult.Value);
            Assert.Equal(mockProfiles.Count, returnValue.Count);
        }
        [Fact]
        public void GetAll_ReturnsUnauthorized_WhenNotAuthorized()
        {

            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "User");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);

            _mockScopedAuthorization.Setup(a => a.Validate(It.IsAny<ClaimsPrincipal>(),
                AuthorizationType.SystemAdmin))
                          .Throws(new UnauthorizedAccessException());

            Assert.Throws<UnauthorizedAccessException>(() => _controller.GetAll());


        }
        [Fact]
        public void GetByEmail_ReturnsOkResult_WithUser()
        {

            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "User");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            _mockScopedAuthorization.Setup(a => a.Validate(It.IsAny<ClaimsPrincipal>(),
               AuthorizationType.SystemAdmin));
            var email = "test@example.com";
            var user = new PersonModel { Email = email, CompanyId = 1 };
            _mockPersonService.Setup(s => s.GetByEmailAsync(email)).ReturnsAsync(user);


            var result = _controller.GetByEmail(email);

            var okResult = Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public void GetById_ReturnsOkResult_WithUser()
        {

            var id = 1;
            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "User");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            _mockScopedAuthorization.Setup(a => a.Validate(It.IsAny<ClaimsPrincipal>(),
               AuthorizationType.SystemAdmin));
            var email = "test@example.com";
            var user = new PersonModel { Email = email, CompanyId = 1 };
            _mockPersonService.Setup(s => s.GetById(id)).Returns(user);

        
            var result = _controller.GetById(id);

          
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PersonModel>(okResult.Value);
            Assert.Equal("test@example.com", returnValue.Email);
        }
        [Fact]
        public void GetDetailsById_ReturnsOkResult_WithUserDetails()
        {
        
            var id = 1;
            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "User");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            _mockScopedAuthorization.Setup(a => a.Validate(It.IsAny<ClaimsPrincipal>(),
               AuthorizationType.SystemAdmin));
            var userDetails = new PersonModel { Id = id, CompanyId = 1, Email = "test@example.com" };
            _mockPersonService.Setup(s => s.GetDetailsById(id)).Returns(userDetails);

            
            var result = _controller.GetDetailsById(id);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PersonModel>(okResult.Value);
            Assert.Equal("test@example.com", returnValue.Email);
        }
        [Fact]
        public async Task Create_ReturnsOkResult_WithMessage()
        {
           
            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "User");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
          
            var model = new PersonModel { CompanyId = 1 };
            _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.User, model.CompanyId));
            _mockPersonService.Setup(service => service.CreateAsync(model)).Returns(Task.FromResult(new Person { }));

           
            var result = _controller.Create(model) as OkObjectResult;

          
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
           
        }
        [Fact]
        public async Task Update_ReturnsOkResult_WithMessage()
        {

            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "User");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            var model = new PersonModel { CompanyId = 1 };
            _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.User, model.CompanyId));
            _mockPersonService.Setup(service => service.Update(It.IsAny<int>(), model));

            var result = _controller.Update(1, model) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
           
        }
        [Fact]
        public async Task Delete_ReturnsOkResult()
        {

            var emailClaim = new Claim(ClaimTypes.Email, "johndoe@example.com");
            var scopeClaim = new Claim("custom:scope", "User");
            var claimsIdentity = new ClaimsIdentity(new[] { emailClaim, scopeClaim });
            var principal = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            int testId = 1;
            _mockPersonService.Setup(service => service.DeleteAsync(testId)).Returns(Task.CompletedTask);

            var result = _controller.Delete(testId);
            Assert.IsType<OkObjectResult>(result);          
            _mockPersonService.Verify(service => service.DeleteAsync(testId), Times.Once);
        }
    }
}
