using Microsoft.AspNetCore.Mvc;
using Moq;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Microsoft.AspNetCore.Identity;
using TelonaiWebApi.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;


namespace TelonaiWebApi.UnitTest.Controllers
{


    public class CompaniesControllerTests
    {
        private readonly Mock<ICompanyService<CompanyModel, Company>> _mockCompanyService;
        private readonly CompaniesController _controller;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IPersonService<PersonModel, Person>> _mockPersonService;
        private readonly Mock<IInvitationService<InvitationModel, Invitation>> _mockInvitationService;
        private readonly Mock<IEmploymentService<EmploymentModel, Employment>> _mockEmploymentService;
        private readonly Mock<IJobService<JobModel, Job>> _mockJobService;
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;

        public CompaniesControllerTests()
        {
            _mockCompanyService = new Mock<ICompanyService<CompanyModel, Company>>();
            _mockUserService = new Mock<IUserService>();
            _mockPersonService = new Mock<IPersonService<PersonModel, Person>>();
            _mockInvitationService = new Mock<IInvitationService<InvitationModel, Invitation>>();
            _mockEmploymentService = new Mock<IEmploymentService<EmploymentModel, Employment>>();
            _mockJobService = new Mock<IJobService<JobModel, Job>>();
            _mockScopedAuthorization = new Mock<IScopedAuthorization>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _controller = new CompaniesController(_mockCompanyService.Object, _mockUserService.Object, _mockPersonService.Object, _mockInvitationService.Object, _mockEmploymentService.Object, _mockJobService.Object, _mockScopedAuthorization.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [Fact]
        public void GetAll_ReturnsOkResult_WithListOfCompanies()
        {

            var companies = new List<CompanyModel>
        {
            new CompanyModel { Id = 1, Name = "Company A" },
            new CompanyModel { Id = 2, Name = "Company B" }
        };
            _mockCompanyService.Setup(service => service.Get()).Returns(companies);


            var result = _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnCompanies = Assert.IsType<List<CompanyModel>>(okResult.Value);
            Assert.Equal(2, returnCompanies.Count);
        }

        [Fact]
        public void GetById_ReturnsOkResult_WithCompany()
        {

            var companyId = 1;
            var company = new CompanyModel { Id = companyId, Name = "Company A" };
            _mockCompanyService.Setup(service => service.GetById(companyId)).Returns(company);


            var result = _controller.GetById(companyId);


            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnCompany = Assert.IsType<CompanyModel>(okResult.Value);
            Assert.Equal(companyId, returnCompany.Id);
        }

        [Fact]
        public void GetJobsByCompanyId_ReturnsOkResult_WithListOfJobs()
        {

            var companyId = 1;
            var jobs = new List<JobModel>
        {
            new JobModel { Id = 1, LocationName = "Job Location A" },
            new JobModel { Id = 2, LocationName = "Job Location B" }
        };
            _mockCompanyService.Setup(service => service.GetJobsById(companyId)).Returns(jobs);


            var result = _controller.GetJobsByCompanyId(companyId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnJobs = Assert.IsType<List<JobModel>>(okResult.Value);
            Assert.Equal(2, returnJobs.Count);
        }

        [Fact]
        public async Task Create_ReturnsOk_WithValidModelAndInvitation()
        {

            var mockUser = new User { Username = "John", Firstname = "John", Lastname = "Doe", Email = "johndoe@example.com", ActivationCode = "validCode" };
            var mockModel = new CompanyRegistrationRequestModel
            {
                Company = new CompanyRequestModel { Name = "Telonai", TaxId = "validTaxId" },
                Manager = mockUser
            };

            _mockInvitationService.Setup(s => s.GetByActivaionCodeAndInviteeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Invitation { TaxId = "validTaxId" });

            _mockCompanyService.Setup(s => s.CreateAsync(It.IsAny<CompanyModel>())).Returns(Task.FromResult(new Company { Id = 1 })); // Simulate company creation

            _mockUserService.Setup(s => s.SignUpAsync(It.IsAny<User>(), It.IsAny<UserRole>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(IdentityResult.Success));

            _mockPersonService.Setup(s => s.CreateAsync(It.IsAny<Person>()))
               .Returns(Task.FromResult(new PersonModel { Id = 1 }));

            _mockJobService.Setup(s => s.CreateAsync(It.IsAny<JobModel>(), It.IsAny<string>(), It.IsAny<string>()))
              .Returns(Task.FromResult(new Job { Id = 1 }));

            _mockEmploymentService.Setup(s => s.CreateAsync(It.IsAny<Employment>(), It.IsAny<int>()))
              .Returns(Task.FromResult(new EmploymentModel { Id = 1 }));


            var result = await _controller.Create(mockModel);


            Assert.IsType<OkObjectResult>(result);
            var loginResult = (LoginResult)((OkObjectResult)result).Value;

        }


        [Fact]
        public async Task Create_ReturnsBadRequest_WithUserSignupFailure()
        {

            var mockModel = new CompanyRegistrationRequestModel
            {
                Company = new CompanyRequestModel { Name = "Telonai", TaxId = "validTaxId" },
                Manager = new User { Username = "aklile" }
            };


            _mockInvitationService.Setup(s => s.GetByActivaionCodeAndInviteeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Invitation { TaxId = "validTaxId" });
            _mockCompanyService.Setup(s => s.CreateAsync(It.IsAny<CompanyModel>())).Returns(Task.FromResult(new Company { Id = 1 }));
            _mockUserService.Setup(s => s.SignUpAsync(It.IsAny<User>(), It.IsAny<UserRole>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "User signup failed" })));

            var result = await _controller.Create(mockModel);

            Assert.IsType<BadRequestResult>(result);
            Assert.NotEmpty(_controller.ModelState);
            Assert.Equal("User signup failed", _controller.ModelState[string.Empty].Errors.First().ErrorMessage);
        }

        [Fact]
        public async Task Update_ReturnsOkResult_WhenUpdateIsSuccessful()
        {

            var id = 1;
            var model = new CompanyModel();
            _mockCompanyService.Setup(s => s.UpdateAsync(id, model)).Returns(Task.CompletedTask);

            var result = await _controller.Update(id, model);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

        }
        [Fact]
        public async Task Delete_ReturnsOkResult_WhenDeleteIsSuccessful()
        {

            var id = 1;
            _mockCompanyService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);


            var result = await _controller.Delete(id);


            Assert.IsType<OkObjectResult>(result);
            _mockCompanyService.Verify(service => service.DeleteAsync(id), Times.Once);
           
        }
        [Fact]
        public void GetSummary_ReturnsOkResult_WithSummary()
        {

            var id = 1;
            var summary = new CompanySummaryModel();
            var claims = new List<Claim>
        {
            new Claim("email", "test@example.com"),
            new Claim("custom:scope", "SystemAdmin")
             };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(principal);
            _mockScopedAuthorization.Setup(a => a.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, id));
            _mockCompanyService.Setup(s => s.GetSummary(id, 4)).Returns(summary);


            var result = _controller.GetSummary(id);


            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(summary, okResult.Value);
        }



    }



}
