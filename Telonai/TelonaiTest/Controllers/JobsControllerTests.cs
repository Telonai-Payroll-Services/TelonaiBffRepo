using AutoMapper;
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
    public class JobsControllerTests
    {
        private readonly JobsController _controller;
        private readonly Mock<IJobService<JobModel, Job>> _mockJobService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        public JobsControllerTests()
        {
            _mockJobService = new Mock<IJobService<JobModel, Job>>();
            _mockMapper = new Mock<IMapper>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockScopedAuthorization = new Mock<IScopedAuthorization>();
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);

            _controller = new JobsController(_mockJobService.Object, _mockMapper.Object, _mockScopedAuthorization.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };

        }



        [Fact]
        public void GetAll_ReturnsOkResult_WithListOfJobs()
        {

            var jobList = new List<JobModel> { new JobModel { Id = 1, LocationName = "Job1" }, new JobModel { Id = 2, LocationName = "Job2" } };

            _mockJobService.Setup(service => service.Get()).Returns(jobList);

            var claims = new List<Claim>
        {
            new Claim("email", "test@example.com"),
            new Claim("custom:scope", "SystemAdmin")
             };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(principal);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);


            _mockScopedAuthorization.Setup(auth => auth.Validate(It.IsAny<ClaimsPrincipal>(), AuthorizationType.SystemAdmin));

            var result = _controller.GetAll();


            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<JobModel>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }
        [Fact]
        public void GetById_ReturnsOkResult_WithJob()
        {

            var jobId = 1;
            var job = new JobModel { Id = jobId, LocationName = "Job1" };
            _mockJobService.Setup(service => service.GetById(jobId)).Returns(job);


            var result = _controller.GetById(jobId);


            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<JobModel>(okResult.Value);
            Assert.Equal(jobId, returnValue.Id);
            Assert.Equal("Job1", returnValue.LocationName);
        }
        [Fact]
        public async Task Create_ReturnsOkResult_WithMessage()
        {

            var model = new JobRequestModel { CompanyId = 1, LocationName = "Job1" };
            var jobModel = new JobModel { Id = 1, LocationName = "Job1" };

            var claims = new List<Claim>
        {
            new Claim("email", "test@example.com"),
            new Claim("username", "testuser"),
            new Claim("custom:scope", "Admin")
        };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.User).Returns(principal);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);

            _mockScopedAuthorization.Setup(auth => auth.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, model.CompanyId));

            _mockMapper.Setup(m => m.Map<JobModel>(model)).Returns(jobModel);

            _mockJobService.Setup(service => service.CreateAsync(jobModel, "testuser", "test@example.com")).Returns(Task.FromResult(new Job { Id = 1, LocationName = "Job1" }));

            var result = _controller.Create(model) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var okResult = Assert.IsType<OkObjectResult>(result);
           


        }
        [Fact]
        public async Task Update_ReturnsOkResult()
        {
           
            int jobId = 1;
            var jobModel = new JobModel();
            var claims = new List<Claim>
        {
            new Claim("email", "test@example.com"),
            new Claim("username", "testuser"),
            new Claim("custom:scope", "Admin")
        };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            _mockJobService.Setup(s => s.UpdateAsync(jobId, jobModel)).Returns(Task.CompletedTask);
            _mockScopedAuthorization.Setup(a => a.ValidateByJobId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, jobId));

            var result = _controller.Update(jobId, jobModel);
        
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
        }


        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            int jobId = 1;
            var claims = new List<Claim>
        {
            new Claim("email", "test@example.com"),
            new Claim("username", "testuser"),
            new Claim("custom:scope", "Admin")
        };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            _mockJobService.Setup(s => s.DeleteAsync(jobId)).Returns(Task.CompletedTask);
            _mockScopedAuthorization.Setup(a => a.Validate(It.IsAny<ClaimsPrincipal>(), AuthorizationType.SystemAdmin));

          
            var result =  _controller.Delete(jobId);

            Assert.IsType<OkObjectResult>(result);
            _mockJobService.Verify(service => service.DeleteAsync(jobId), Times.Once);
          
        }
    }
}
