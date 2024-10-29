using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
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
  
        var result = _service.Get();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Company A", result[0].Name);
        Assert.Equal("Company B", result[1].Name);

        _mockContext.Reset();
    }

    [Fact]
    public void GetJobsById_ReturnsMappedJobs_WhenJobsExistForCompany()
    {

        int companyId = 456;

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

        var jobModels = new List<JobModel> { new JobModel { CompanyId = companyId } };
        _mockMapper.Setup(m => m.Map<IList<JobModel>>(It.IsAny<List<Job>>())).Returns(jobModels);

        var result = _service.GetJobsById(companyId);

        Assert.NotNull(result);
        Assert.Equal(1, result.Count);
        _mockContext.Reset();

    }
    [Fact]
    public void GetSummary_ShouldReturnCorrectSummary()
    {

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

        var result = _service.GetSummary(companyId, count);

        Assert.NotNull(result);
        Assert.Equal(3, result.NumberOfEmployees);
        Assert.Equal(1, result.NumberOfLocations);
        Assert.Equal(payrolls, result.Payrolls);
    }
    [Fact]
    public void GetById_ShouldReturnCompanyModel_WhenCompanyExists()
    {

        var companyId = 1;
        var company = new Company { Id = companyId, Name = "Test Company" };
        var companyModel = new CompanyModel { Id = companyId, Name = "Test Company" };

        _mockContext.Setup(c => c.Company.Find(companyId))
           .Returns(company);

        _mockMapper.Setup(mapper => mapper.Map<CompanyModel>(company)).Returns(companyModel);

        var result = _service.GetById(companyId);

        Assert.NotNull(result);
        Assert.Equal(companyId, result.Id);
        Assert.Equal("Test Company", result.Name);
    }

    [Fact]
    public async Task CreateAsync_CreatesCompany_WhenTaxIdUnique()
    {
        
        var companies = new List<Company>
         {
            new Company { Id = 1, TaxId = "12345existingTaxId", Name = "existing Company" }
         }.AsQueryable();
        var mockSet = new Mock<DbSet<Company>>();
        mockSet.As<IQueryable<Company>>().Setup(m => m.Provider).Returns(companies.Provider);
        mockSet.As<IQueryable<Company>>().Setup(m => m.Expression).Returns(companies.Expression);
        mockSet.As<IQueryable<Company>>().Setup(m => m.ElementType).Returns(companies.ElementType);
        mockSet.As<IQueryable<Company>>().Setup(m => m.GetEnumerator()).Returns(companies.GetEnumerator());

        _mockContext.Setup(c => c.Company).Returns(mockSet.Object);

        var companyModel = new CompanyModel { TaxId = "uniqueTaxId" };
        var company = new Company { TaxId = "uniqueTaxId" };
        _mockMapper.Setup(m => m.Map<Company>(It.IsAny<CompanyModel>())).Returns(company);
        var createdCompany = await _service.CreateAsync(companyModel);

        Assert.NotNull(createdCompany);
        Assert.Equal(companyModel.TaxId, createdCompany.TaxId);

    }
    [Fact]
    public async Task CreateAsync_ReturnsExistingCompany_WhenTaxIdExists()
    {

        var companies = new List<Company>
         {
            new Company { Id = 1, TaxId = "12345existingTaxId", Name = "existing Company" }
         }.AsQueryable();
        var mockSet = new Mock<DbSet<Company>>();
        mockSet.As<IQueryable<Company>>().Setup(m => m.Provider).Returns(companies.Provider);
        mockSet.As<IQueryable<Company>>().Setup(m => m.Expression).Returns(companies.Expression);
        mockSet.As<IQueryable<Company>>().Setup(m => m.ElementType).Returns(companies.ElementType);
        mockSet.As<IQueryable<Company>>().Setup(m => m.GetEnumerator()).Returns(companies.GetEnumerator());

        _mockContext.Setup(c => c.Company).Returns(mockSet.Object);

        var companyModel = new CompanyModel { TaxId = "12345existingTaxId" };
        var createdCompany = await _service.CreateAsync(companyModel);

        Assert.NotNull(createdCompany);
        Assert.Equal(companies.FirstOrDefault().Id, createdCompany.Id);
    }
    [Fact]
    public async Task UpdateAsync_UpdatesCompany_WhenFound()
    {
        
        var existingCompany = new Company { Id = 1, Name = "Company A" };
        _mockContext.Setup(dc => dc.Company.Find(It.IsAny<int>()))
            .Returns(existingCompany); 
        _mockContext.Setup(dc => dc.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

        var updateModel = new CompanyModel { Id = 1, Name = "Updated Name" };
        _mockMapper.Setup(m => m.Map(updateModel, existingCompany))
     .Callback<CompanyModel, Company>((src, dest) => dest.Name = src.Name);
        await _service.UpdateAsync(1, updateModel);
     
        Assert.Equal("Updated Name", existingCompany.Name);
    }
    [Fact]
    public async Task UpdateAsync_ThrowsException_WhenCompanyNotFound()
    {

        var companyId = 1;
        _mockContext.Setup(dc => dc.Company.Find(It.IsAny<int>()))
            .Returns((Company)null); 


        var updateModel = new CompanyModel { Id = 1, Name = "Updated Name" };

        var exception = await Assert.ThrowsAsync<AppException>(() => _service.UpdateAsync(1, updateModel));
        Assert.Equal("Company not found", exception.Message);
    }
    [Fact]
    public async Task DeleteAsync_DeletesCompany_WhenFound()
    {
     
        var existingCompany = new Company { Id = 1, Name = "Company A" };
        _mockContext.Setup(dc => dc.Company.Find(It.Is<int>(id => id == 1)))
            .Returns(existingCompany); 

        _mockContext.Setup(dc => dc.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); 

      
        await _service.DeleteAsync(1);

        _mockContext.Verify(dc => dc.Company.Remove(existingCompany), Times.Once);
        
    }
    [Fact]
    public async Task DeleteAsync_ThrowsAppException_WhenCompanyNotFound()
    {
       
        _mockContext.Setup(dc => dc.Company.Find(It.IsAny<int>()))
            .Returns((Company)null); 
    
        var exception = await Assert.ThrowsAsync<AppException>(() => _service.DeleteAsync(1));
        Assert.Equal("Company not found", exception.Message);

    }
}