using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

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
        // Arrange
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(-1),
            CompanyId = 1
        };

        // Act & Assert
        var exception = Assert.Throws<AppException>(() => _service.Create(model));
        Assert.Equal("Payroll cannot be scheduled for the past", exception.Message);
    }

    [Fact]
    public void Create_ThrowsException_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now,
            CompanyId = 1
        };

        // Act & Assert
        var exception = Assert.Throws<AppException>(() => _service.Create(model));
        Assert.Equal("Invalid payroll start-date or end-date", exception.Message);
    }

    [Fact]
    public void Create_ThrowsException_WhenFirstRunDateIsBeforeStartDate()
    {
        // Arrange
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            FirstRunDate = DateTime.Now,
            CompanyId = 1
        };

        // Act & Assert
        var exception = Assert.Throws<AppException>(() => _service.Create(model));
        Assert.Equal("Invalid payroll first-run date", exception.Message);
    }

    [Fact]
    public void Create_UpdatesCurrentScheduleAndAddsNewSchedule1()
    {
        // Arrange
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            FirstRunDate = DateTime.Now.AddDays(2),
            CompanyId = 1
        };

        var now = DateOnly.FromDateTime(DateTime.Now);
        var currentSchedule = new PayrollSchedule
        {
            CompanyId = model.CompanyId,
            StartDate = now.AddDays(-10),
            EndDate = now.AddDays(5)
        };

        var mockDbSet = new Mock<DbSet<PayrollSchedule>>();
        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockDbSet.Object);

        var mockEmploymentDbSet = new Mock<DbSet<Employment>>();
        _mockContext.Setup(c => c.Employment).Returns(mockEmploymentDbSet.Object);

        var mockPayrollDbSet = new Mock<DbSet<Payroll>>();
        _mockContext.Setup(c => c.Payroll).Returns(mockPayrollDbSet.Object);

        _mockContext.Setup(c => c.PayrollSchedule.OrderByDescending(e => e.StartDate))
            .Returns(new List<PayrollSchedule> { currentSchedule }.AsQueryable().OrderByDescending(e => e.StartDate));

        // Act
        _service.Create(model);

        // Assert
        _mockContext.Verify(c => c.PayrollSchedule.Update(It.IsAny<PayrollSchedule>()), Times.Once);
        _mockContext.Verify(c => c.PayrollSchedule.Add(It.IsAny<PayrollSchedule>()), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));

    }
    [Fact]
    public void Create_UpdatesCurrentScheduleAndAddsNewSchedule()
    {
        // Arrange
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            FirstRunDate = DateTime.Now.AddDays(2),
            CompanyId = 1
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

        var mockPayrollDbSet = new Mock<DbSet<Payroll>>();
        _mockContext.Setup(c => c.Payroll).Returns(mockPayrollDbSet.Object);

        // Act
        _service.Create(model);

        // Assert
        _mockContext.Verify(c => c.PayrollSchedule.Update(It.IsAny<PayrollSchedule>()), Times.Once);
        _mockContext.Verify(c => c.PayrollSchedule.Add(It.IsAny<PayrollSchedule>()), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));
    }

    [Fact]
    public void Create_HandlesCase_WhenNoCurrentSchedule()
    {
        // Arrange
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            FirstRunDate = DateTime.Now.AddDays(2),
            CompanyId = 1
        };

        var payrollSchedules = new List<PayrollSchedule>().AsQueryable();

        var mockDbSet = new Mock<DbSet<PayrollSchedule>>();
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(payrollSchedules.Provider);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(payrollSchedules.Expression);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(payrollSchedules.ElementType);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(payrollSchedules.GetEnumerator());

        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockDbSet.Object);

        var mockEmploymentDbSet = new Mock<DbSet<Employment>>();
        _mockContext.Setup(c => c.Employment).Returns(mockEmploymentDbSet.Object);

        var mockPayrollDbSet = new Mock<DbSet<Payroll>>();
        _mockContext.Setup(c => c.Payroll).Returns(mockPayrollDbSet.Object);

        // Act
        _service.Create(model);

        // Assert
        _mockContext.Verify(c => c.PayrollSchedule.Add(It.IsAny<PayrollSchedule>()), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));
    }

    [Fact]
    public void Create_HandlesCase_WhenNoActiveEmployment()
    {
        // Arrange
        var model = new PayrollScheduleModel
        {
            StartDate = DateTime.Now.AddDays(1),
            FirstRunDate = DateTime.Now.AddDays(2),
            CompanyId = 1
        };

        var payrollSchedules = new List<PayrollSchedule>().AsQueryable();
        var employments = new List<Employment>().AsQueryable();  // No active employment

        var mockDbSet = new Mock<DbSet<PayrollSchedule>>();
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(payrollSchedules.Provider);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(payrollSchedules.Expression);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(payrollSchedules.ElementType);
        mockDbSet.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(payrollSchedules.GetEnumerator());

        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockDbSet.Object);

        var mockEmploymentDbSet = new Mock<DbSet<Employment>>();
        mockEmploymentDbSet.As<IQueryable<Employment>>().Setup(m => m.Provider).Returns(employments.Provider);
        mockEmploymentDbSet.As<IQueryable<Employment>>().Setup(m => m.Expression).Returns(employments.Expression);
        mockEmploymentDbSet.As<IQueryable<Employment>>().Setup(m => m.ElementType).Returns(employments.ElementType);
        mockEmploymentDbSet.As<IQueryable<Employment>>().Setup(m => m.GetEnumerator()).Returns(employments.GetEnumerator());
        _mockContext.Setup(c => c.Employment).Returns(mockEmploymentDbSet.Object);

        var mockPayrollDbSet = new Mock<DbSet<Payroll>>();
        _mockContext.Setup(c => c.Payroll).Returns(mockPayrollDbSet.Object);

        // Act
        _service.Create(model);

        // Assert
        _mockContext.Verify(c => c.PayrollSchedule.Add(It.IsAny<PayrollSchedule>()), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));
    }


    [Fact]
    public void Update_ThrowsException_WhenPayrollScheduleNotFound()
    {
        // Arrange
        int id = 1;
        var model = new PayrollScheduleModel { /* properties */ };
        _mockContext.Setup(c => c.PayrollSchedule.Find(id)).Returns((PayrollSchedule)null);

        // Act & Assert
        var exception = Assert.Throws<AppException>(() => _service.Update(id, model));
        Assert.Equal("Payroll Schedule not found", exception.Message);
    }

    [Fact]
    public void Update_UpdatesPayrollSchedule()
    {
        // Arrange
        int id = 1;
        var model = new PayrollScheduleModel { /* properties */ };
        var existingSchedule = new PayrollSchedule { Id = id, /* other properties */ };

        _mockContext.Setup(c => c.PayrollSchedule.Find(id)).Returns(existingSchedule);
        _mockMapper.Setup(m => m.Map(model, existingSchedule));

        // Act
        _service.Update(id, model);

        // Assert
        _mockContext.Verify(c => c.PayrollSchedule.Update(existingSchedule), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Once);
    }

    [Fact]
    public void Delete_RemovesPayrollSchedule()
    {
        // Arrange
        int id = 1;
        var existingSchedule = new PayrollSchedule { Id = id, /* other properties */ };

        _mockContext.Setup(c => c.PayrollSchedule.Find(id)).Returns(existingSchedule);

        // Act
        _service.Delete(id);

        // Assert
        _mockContext.Verify(c => c.PayrollSchedule.Remove(existingSchedule), Times.Once);
        _mockContext.Verify(c => c.SaveChanges(), Times.Once);

    }
}
