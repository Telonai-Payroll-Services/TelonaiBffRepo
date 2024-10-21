using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

public class PayrollScheduleServiceTests
{
    private readonly Mock<DataContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PayrollScheduleService _service;

    public PayrollScheduleServiceTests()
    {
        _mockContext = new Mock<DataContext>();
        _mockMapper = new Mock<IMapper>();
        _service = new PayrollScheduleService(_mockContext.Object, _mockMapper.Object);
    }

    [Fact]
    public void GetLatestByCompanyId_ReturnsExpectedResult()
    {
        int companyId = 1;
        var now = DateTime.Now;
        var payrollSchedules = new List<PayrollSchedule>
        {
            new PayrollSchedule { CompanyId = companyId, StartDate = DateOnly.FromDateTime(now.AddDays(-10)), EndDate = DateOnly.FromDateTime(now.AddDays(-5)) },
            new PayrollSchedule { CompanyId = companyId, StartDate = DateOnly.FromDateTime(now.AddDays(-20)), EndDate = DateOnly.FromDateTime(now.AddDays(-15)) },
            new PayrollSchedule { CompanyId = companyId, StartDate = DateOnly.FromDateTime(now.AddDays(-30)), EndDate = DateOnly.FromDateTime(now.AddDays(5)) } // this should be selected
        }.AsQueryable();

        var mockDbSet = new Mock<DbSet<PayrollSchedule>>();
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(payrollSchedules.Provider);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(payrollSchedules.Expression);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(payrollSchedules.ElementType);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(payrollSchedules.GetEnumerator());

        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockDbSet.Object);

        var expectedPayrollScheduleModel = new PayrollScheduleModel { CompanyId = companyId, StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(5) };
        _mockMapper.Setup(m => m.Map<PayrollScheduleModel>(It.IsAny<PayrollSchedule>())).Returns(expectedPayrollScheduleModel);

        var result = _service.GetLatestByCompanyId(companyId);

        Assert.Equal(expectedPayrollScheduleModel, result);
    }
    [Fact]
    public void GetLatestByCompanyId_ReturnsNull_WhenNotFound()
    {

        int companyId = 1;
        var payrollSchedules = new List<PayrollSchedule>().AsQueryable();

        var mockDbSet = new Mock<DbSet<PayrollSchedule>>();
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(payrollSchedules.Provider);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(payrollSchedules.Expression);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(payrollSchedules.ElementType);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(payrollSchedules.GetEnumerator());

        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockDbSet.Object);

        var result = _service.GetLatestByCompanyId(companyId);


        Assert.Null(result);
    }
    [Fact]
    public void GetByCompanyId_ReturnsExpectedResult()
    {
        int companyId = 1;
        var payrollSchedules = new List<PayrollSchedule>
        {
            new PayrollSchedule { CompanyId = companyId, StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)) },
            new PayrollSchedule { CompanyId = companyId, StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)) },
            new PayrollSchedule { CompanyId = companyId, StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)) }
        }.AsQueryable();

        var mockDbSet = new Mock<DbSet<PayrollSchedule>>();
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(payrollSchedules.Provider);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(payrollSchedules.Expression);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(payrollSchedules.ElementType);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(payrollSchedules.GetEnumerator());

        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockDbSet.Object);

        var expectedPayrollScheduleModels = new List<PayrollScheduleModel>
        {
            new PayrollScheduleModel { CompanyId = companyId, StartDate = DateTime.Now.AddDays(-10) },
            new PayrollScheduleModel { CompanyId = companyId, StartDate = DateTime.Now.AddDays(-20) },
            new PayrollScheduleModel { CompanyId = companyId, StartDate = DateTime.Now.AddDays(-30) }
        };
        _mockMapper.Setup(m => m.Map<List<PayrollScheduleModel>>(It.IsAny<IQueryable<PayrollSchedule>>())).Returns(expectedPayrollScheduleModels);

        var result = _service.GetByCompanyId(companyId);

        Assert.Equal(expectedPayrollScheduleModels, result);
    }

    [Fact]
    public void GetByCompanyId_ReturnsEmptyList_WhenNotFound()
    {
        int companyId = 1;
        var payrollSchedules = new List<PayrollSchedule>().AsQueryable();

        var mockDbSet = new Mock<DbSet<PayrollSchedule>>();
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(payrollSchedules.Provider);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(payrollSchedules.Expression);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(payrollSchedules.ElementType);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(payrollSchedules.GetEnumerator());

        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockDbSet.Object);

        var expectedPayrollScheduleModels = new List<PayrollScheduleModel>();
        _mockMapper.Setup(m => m.Map<List<PayrollScheduleModel>>(It.IsAny<IQueryable<PayrollSchedule>>())).Returns(expectedPayrollScheduleModels);

        var result = _service.GetByCompanyId(companyId);
        Assert.Empty(result);
    }

    [Fact]
    public void GetById_ReturnsExpectedResult()
    {
        int id = 1;
        var payrollSchedule = new PayrollSchedule { Id = id, StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)) };
        _mockContext.Setup(c => c.PayrollSchedule.Find(id)).Returns(payrollSchedule);

        var expectedPayrollScheduleModel = new PayrollScheduleModel { StartDate = DateTime.Now.AddDays(-20) };
        _mockMapper.Setup(m => m.Map<PayrollScheduleModel>(It.IsAny<PayrollSchedule>())).Returns(expectedPayrollScheduleModel);

        var result = _service.GetById(id);

        Assert.Equal(expectedPayrollScheduleModel, result);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenNotFound()
    {
        int id = 1;
        _mockContext.Setup(c => c.PayrollSchedule.Find(id)).Returns((PayrollSchedule)null);

        var result = _service.GetById(id);

        Assert.Null(result);
    }


    [Fact]
    public void Create_ThrowsException_WhenStartDateIsInThePast()
    {
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(-1),
            CompanyId = 1
        };

        var exception = Assert.Throws<AppException>(() => _service.Create(model));
        Assert.Equal("Payroll cannot be scheduled for the past", exception.Message);
    }

    [Fact]
    public void Create_ThrowsException_WhenEndDateIsBeforeStartDate()
    {
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now,
            CompanyId = 1
        };

        var exception = Assert.Throws<AppException>(() => _service.Create(model));
        Assert.Equal("Invalid payroll start-date or end-date", exception.Message);
    }

    [Fact]
    public void Create_ThrowsException_WhenFirstRunDateIsBeforeStartDate()
    {
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            FirstRunDate = DateTime.Now,
            CompanyId = 1
        };

        var exception = Assert.Throws<AppException>(() => _service.Create(model));
        Assert.Equal("Invalid payroll first-run date", exception.Message);
    }
    [Fact]
    public void Create_UpdatesCurrentScheduleAndAddsNewSchedule()
    {
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            FirstRunDate = DateTime.Now.AddDays(2),
            CompanyId = 1,
            PayrollScheduleType= "Monthly"
        };

        var now = DateOnly.FromDateTime(DateTime.Now);
        var currentSchedule = new PayrollSchedule
        {
            CompanyId = model.CompanyId,
            StartDate = now.AddDays(-10),
            EndDate = now.AddDays(5)
        };

        var payrollSchedules = new List<PayrollSchedule> { currentSchedule }.AsQueryable();

        var mockDbSet = new Mock<DbSet<PayrollSchedule>>();
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(payrollSchedules.Provider);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(payrollSchedules.Expression);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(payrollSchedules.ElementType);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(payrollSchedules.GetEnumerator());

        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockDbSet.Object);

        var mockEmploymentDbSet = new Mock<DbSet<Employment>>();
        _mockContext.Setup(c => c.Employment).Returns(mockEmploymentDbSet.Object);
        var payroll = new Payroll
        {
            CompanyId = model.CompanyId,
            StartDate = now.AddDays(-10),
            
        };

        var payrolls = new List<Payroll> { payroll }.AsQueryable();

        var mockPayrollDbSet = new Mock<DbSet<Payroll>>();
        mockPayrollDbSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrolls.Provider);
        mockPayrollDbSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrolls.Expression);
        mockPayrollDbSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrolls.ElementType);
        mockPayrollDbSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrolls.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockPayrollDbSet.Object);

        _service.Create(model);

        _mockContext.Verify(c => c.PayrollSchedule.Update(It.IsAny<PayrollSchedule>()), Times.Once);
        _mockContext.Verify(c => c.PayrollSchedule.Add(It.IsAny<PayrollSchedule>()), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));
    }

    [Fact]
    public void Create_CheckFirstScheduleAndUpdateStatuse_WhenNoCurrentScheduleIsFound()
    {
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            FirstRunDate = DateTime.Now.AddDays(2),
            CompanyId = 1,
             PayrollScheduleType = "Monthly"
        };

        var payrollSchedules = new List<PayrollSchedule>().AsQueryable();

        var mockDbSet = new Mock<DbSet<PayrollSchedule>>();
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(payrollSchedules.Provider);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(payrollSchedules.Expression);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(payrollSchedules.ElementType);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(payrollSchedules.GetEnumerator());

        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockDbSet.Object);



        var employments = new List<Employment>
        { new Employment { Job = new Job { CompanyId = 1, Id = 3 }, Deactivated = false,PayRateBasisId=4,PayRate=10000 }
        }.AsQueryable();

        var mockEmploymentDbSet = new Mock<DbSet<Employment>>();
        mockEmploymentDbSet.As<IQueryable<Employment>>().Setup(m => m.Provider).Returns(employments.Provider);
        mockEmploymentDbSet.As<IQueryable<Employment>>().Setup(m => m.Expression).Returns(employments.Expression);
        mockEmploymentDbSet.As<IQueryable<Employment>>().Setup(m => m.ElementType).Returns(employments.ElementType);
        mockEmploymentDbSet.As<IQueryable<Employment>>().Setup(m => m.GetEnumerator()).Returns(employments.GetEnumerator());
        _mockContext.Setup(c => c.Employment).Returns(mockEmploymentDbSet.Object);

        var payroll = new Payroll
        {
            CompanyId = model.CompanyId,
            StartDate = DateOnly.FromDateTime(DateTime.Now).AddDays(-10),

        };

        var payrolls = new List<Payroll> { payroll }.AsQueryable();

        var mockPayrollDbSet = new Mock<DbSet<Payroll>>();
        mockPayrollDbSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrolls.Provider);
        mockPayrollDbSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrolls.Expression);
        mockPayrollDbSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrolls.ElementType);
        mockPayrollDbSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrolls.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockPayrollDbSet.Object);

        _service.Create(model);
        
        _mockContext.Verify(c => c.PayrollSchedule.Add(It.IsAny<PayrollSchedule>()), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));
    }

    


    [Fact]
    public void Update_ThrowsException_WhenPayrollScheduleNotFound()
    {
        int id = 1;
        var model = new PayrollScheduleModel {  };
        _mockContext.Setup(c => c.PayrollSchedule.Find(id)).Returns((PayrollSchedule)null);

  
        var exception = Assert.Throws<AppException>(() => _service.Update(id, model));
        Assert.Equal("Payroll Schedule not found", exception.Message);
    }

    [Fact]
    public void Update_UpdatesPayrollSchedule()
    {
        int id = 1;
        var model = new PayrollScheduleModel {  };
        var existingSchedule = new PayrollSchedule { Id = id, };

        _mockContext.Setup(c => c.PayrollSchedule.Find(id)).Returns(existingSchedule);
        _mockMapper.Setup(m => m.Map(model, existingSchedule));

        _service.Update(id, model);


        _mockContext.Verify(c => c.PayrollSchedule.Update(existingSchedule), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Once);
    }

    [Fact]
    public void Delete_RemovesPayrollSchedule()
    {

        int id = 1;
        var existingSchedule = new PayrollSchedule { Id = id,  };

        _mockContext.Setup(c => c.PayrollSchedule.Find(id)).Returns(existingSchedule);

        _service.Delete(id);

        _mockContext.Verify(c => c.PayrollSchedule.Remove(existingSchedule), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Once);

    }
}
