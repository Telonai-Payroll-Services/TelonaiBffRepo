
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using System;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using System.Reflection;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

public class CompanyServiceTests
{
    private readonly Mock<DataContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CompanyService _service;
    private readonly Mock<IPayrollService> _mockPayrollService;

    public CompanyServiceTests()
    {
        _mockContext = new Mock<DataContext>(MockBehavior.Default, new object[] { new Mock<IHttpContextAccessor>().Object });
        _mockMapper = new Mock<IMapper>();
        _mockPayrollService = new Mock<IPayrollService>();
        _service = new CompanyService(_mockContext.Object, _mockMapper.Object, null, _mockPayrollService.Object);
    }

    [Fact]
    public void GetCompany_ReturnsAllMappedCompaniesFromDatabase()
    {
        // Arrange
       
      
        var companies = new List<Company>
        {
            new Company { Id = 1, Name = "Company A" },
            new Company { Id = 2, Name = "Company B" }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Company>>();
        mockSet.As<IQueryable<Company>>().Setup(m => m.Provider).Returns(companies.Provider);
        mockSet.As<IQueryable<Company>>().Setup(m => m.Expression).Returns(companies.Expression);
        mockSet.As<IQueryable<Company>>().Setup(m => m.ElementType).Returns(companies.ElementType);
        mockSet.As<IQueryable<Company>>().Setup(m => m.GetEnumerator()).Returns(companies.GetEnumerator());
        var companyModels = new List<CompanyModel>
        {
            new CompanyModel { Id = 1, Name = "Company A" },
            new CompanyModel { Id = 2, Name = "Company B" }
        };
        _mockContext.Setup(c => c.Company).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChanges()).Returns(1);
        _mockMapper.Setup(m => m.Map<IList<CompanyModel>>(It.IsAny<IList<Company>>())).Returns(companyModels);
        // Act
        var result = _service.Get();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Company A", result[0].Name);
        Assert.Equal("Company B", result[1].Name);
       
        _mockContext.Reset();
    }
    
    [Fact]
    public void GetJobsById_ReturnsMappedJobs_WhenJobsExistForCompany()
    {
        // Arrange
        
        int companyId = 456;


        // Mock DbContext behavior: Return sample jobs
        var jobs = new List<Job>()
        {
            new Job { Id = 1, CompanyId = companyId, LocationName = "north x",AddressLine1="north carolina" }
            
        }.AsQueryable();
        var mockSet = new Mock<DbSet<Job>>();
        mockSet.As<IQueryable<Job>>().Setup(m => m.Provider).Returns(jobs.Provider);
        mockSet.As<IQueryable<Job>>().Setup(m => m.Expression).Returns(jobs.Expression);
        mockSet.As<IQueryable<Job>>().Setup(m => m.ElementType).Returns(jobs.ElementType);
        mockSet.As<IQueryable<Job>>().Setup(m => m.GetEnumerator()).Returns(jobs.GetEnumerator());
        _mockContext.Setup(c => c.Job).Returns(mockSet.Object);
       
        var jobModels = new List<JobModel> { new JobModel{ CompanyId = companyId } };
        _mockMapper.Setup(m => m.Map<IList<JobModel>>(It.IsAny<List<Job>>())).Returns(jobModels);


        // Act
        var result = _service.GetJobsById(companyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Count); 
        _mockContext.Reset();

    }
    [Fact]
    public void GetSummary_ShouldReturnCorrectSummary()
    {
        // Arrange
        var companyId = 1;
        var count = 5;

        var payrolls = new List<PayrollModel> { new PayrollModel(), new PayrollModel() };
        _mockPayrollService.Setup(p => p.GetLatestByCount(companyId, count)).Returns(payrolls);

        var employments = new List<Employment>
        { new Employment { Job = new Job { CompanyId = companyId, Id = 1 }, Deactivated = false },
            new Employment { Job = new Job { CompanyId = companyId, Id = 2 }, Deactivated = false },
            new Employment { Job = new Job { CompanyId = companyId, Id = 1 }, Deactivated = false }
        }.AsQueryable();
        var mockEmploymentSet = new Mock<DbSet<Employment>>();
        mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.Provider).Returns(employments.Provider);
        mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.Expression).Returns(employments.Expression);
        mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.ElementType).Returns(employments.ElementType);
        mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.GetEnumerator()).Returns(employments.GetEnumerator());
        _mockContext.Setup(c => c.Employment).Returns(mockEmploymentSet.Object);

        // Act
        var result = _service.GetSummary(companyId, count);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.NumberOfEmployees);
        Assert.Equal(1, result.NumberOfLocations);
        Assert.Equal(payrolls, result.Payrolls);
    }
    [Fact]
    public void GetById_ShouldReturnCompanyModel_WhenCompanyExists()
    {
        // Arrange
        var companyId = 1;
        var company = new Company { Id = companyId, Name = "Test Company" };
        var companyModel = new CompanyModel { Id = companyId, Name = "Test Company" };
        var companies = new List<Company>
        {
            new Company { Id = 1, Name = "Test Company" }
          
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Company>>();
        mockSet.As<IQueryable<Company>>().Setup(m => m.Provider).Returns(companies.Provider);
        mockSet.As<IQueryable<Company>>().Setup(m => m.Expression).Returns(companies.Expression);
        mockSet.As<IQueryable<Company>>().Setup(m => m.ElementType).Returns(companies.ElementType);
        mockSet.As<IQueryable<Company>>().Setup(m => m.GetEnumerator()).Returns(companies.GetEnumerator());
      
        _mockContext.Setup(c => c.Company).Returns(mockSet.Object);
   
      

        var methodInfo = typeof(CompanyService).GetMethod("GetCompany", BindingFlags.NonPublic | BindingFlags.Instance);
       var result1= methodInfo.Invoke(_service, new object[] { companyId });

        _mockMapper.Setup(mapper => mapper.Map<CompanyModel>(company)).Returns(companyModel);

        // Act
        var result = _service.GetById(companyId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(companyId, result.Id);
        Assert.Equal("Test Company", result.Name);
    }

   
}
