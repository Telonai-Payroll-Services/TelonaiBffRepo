using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;


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

        public CompaniesControllerTests()
        {
            _mockCompanyService = new Mock<ICompanyService<CompanyModel, Company>>();
            _mockUserService = new Mock<IUserService>();
            _mockPersonService = new Mock<IPersonService<PersonModel, Person>>();
            _mockInvitationService = new Mock<IInvitationService<InvitationModel, Invitation>>();
            _mockEmploymentService = new Mock<IEmploymentService<EmploymentModel, Employment>>();
            _mockJobService = new Mock<IJobService<JobModel, Job>>();
            _controller = new CompaniesController(_mockCompanyService.Object, _mockUserService.Object, _mockPersonService.Object, _mockInvitationService.Object, _mockEmploymentService.Object, _mockJobService.Object);
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
        public async Task Create_ReturnsBadRequest_WithInvalidModel()
        {
           
            var mockModel = new CompanyRegistrationRequestModel(); 


          
            var result = await _controller.Create(mockModel);

           
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotEmpty(_controller.ModelState); 
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WithMissingInvitation()
        {
            // Arrange
            var mockModel = new CompanyRegistrationRequestModel
            {
                Company = new CompanyRequestModel { Name = "Telonai", TaxId = "validTaxId" },
                Manager = new User { Username = "aklile" }
            };

            _mockInvitationService.Setup(s => s.GetByActivaionCodeAndInviteeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((Invitation)null); 


            
            var result = await _controller.Create(mockModel);

           
            Assert.IsType<BadRequestObjectResult>(result);

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
    }
        

}
