using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using System.Reflection;
using System.Linq.Expressions;
using System.Net.Sockets;
using Amazon.SimpleEmail.Model;
using System.ComponentModel.Design;

public class PayrollServiceTests
{
    private readonly Mock<DataContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PayrollService _payrollService;

    public PayrollServiceTests()
    {
        _mockContext = new Mock<DataContext>();
        _mockMapper = new Mock<IMapper>();
        _payrollService = new PayrollService(_mockContext.Object, _mockMapper.Object);
    }

    [Fact]
    public void GetLatestByCount_ShouldReturnCorrectPayrollModels()
    {
       
        int companyId = 1;
        int count = 4;
        var payrolls = new List<Payroll>
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), EmployeesOwed = 600 },
            new Payroll { CompanyId = companyId, ScheduledRunDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-11)), EmployeesOwed = 2400 },
            new Payroll { CompanyId = companyId, ScheduledRunDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)), StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-12)), EmployeesOwed = 3000 },
            new Payroll { CompanyId = companyId, ScheduledRunDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-12)), EmployeesOwed = 4000 }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrolls.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrolls.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrolls.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrolls.GetEnumerator());

        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);
        _mockMapper.Setup(m => m.Map<List<PayrollModel>>(It.IsAny<List<Payroll>>())).Returns(new List<PayrollModel>
        {
            new PayrollModel { ScheduledRunDate = DateTime.UtcNow.AddDays(-1), EmployeesOwed = 600 },
            new PayrollModel { ScheduledRunDate = DateTime.UtcNow.AddDays(-2), EmployeesOwed = 2400 },
            new PayrollModel { ScheduledRunDate = DateTime.UtcNow.AddDays(-3), EmployeesOwed = 3000 },
            new PayrollModel { ScheduledRunDate = DateTime.UtcNow.AddDays(3), EmployeesOwed = 4000 }
        });

       
        var result = _payrollService.GetLatestByCount(companyId, count);

       
        Assert.Equal(4, result.Count);
        Assert.Equal("#3CA612", result[0].ExpenseTrackingHexColor);
        Assert.Equal("#FCCC44", result[1].ExpenseTrackingHexColor);
        Assert.Equal("#D20103", result[2].ExpenseTrackingHexColor);
        Assert.Equal("#D20103", result[3].ExpenseTrackingHexColor);
    }
 


    [Fact]
    public void GetDailyPayrollExpense_ShouldReturnCorrectDailyAmount()
    {
        double totalEmployeesOwed = 1000;
        DateTime startDate = DateTime.UtcNow.AddDays(-10);
        DateTime endDate = DateTime.UtcNow;
        var span = endDate - startDate;

        MethodInfo methodInfo = typeof(PayrollService).GetMethod("GetDailyPayrollExpense", BindingFlags.NonPublic | BindingFlags.Static);

        object result = methodInfo.Invoke(null, new object[] { totalEmployeesOwed, startDate, endDate });
        
        double expectedDailyAmount = totalEmployeesOwed / span.TotalDays;
        Assert.Equal(expectedDailyAmount, result);
    }
    [Fact]
    public void GetReport_ReturnsMappedPayrollModels_WhenDataExists()
    {
        int companyId = 1;
        DateOnly from = new DateOnly(2024, 10, 15);
        DateOnly to = new DateOnly(2024, 10, 20);

        var payrollData = new List<Payroll>
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = new DateOnly(2024, 10, 18) },
        }.AsQueryable(); ;
        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
       

        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);
       
        var expectedPayrollModels = new List<PayrollModel>
        {
            
            new PayrollModel(), 
        };
        _mockMapper.Setup(m => m.Map<List<PayrollModel>>(It.IsAny<List<Payroll>>())).Returns(expectedPayrollModels);

        var result = _payrollService.GetReport(companyId, from, to);

        Assert.Equal(expectedPayrollModels, result);
    }
    [Fact]
    public void GetReport_ReturnsNull_WhenNoDataExists()
    {
        int companyId = 1;
        DateOnly from = new DateOnly(2024, 10, 15);
        DateOnly to = new DateOnly(2024, 10, 20);
        var payrollData = new List<Payroll> { };
       
        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(new Mock<IQueryProvider>().Object);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(Expression.Constant(payrollData.AsQueryable()));
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(typeof(Payroll));
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());

        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);  
        
        var result =_payrollService.GetReport(companyId, from, to);

        Assert.Null(result);
    }
 
    [Fact]
    public void GetCurrentPayroll_ReturnsMappedModel_WhenPayrollExists()
    {

        int companyId = 1;
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var payrollData = new List<Payroll>()
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-2), TrueRunDate = null },
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-1), StartDate = today, TrueRunDate = null },
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var expectedModel = new PayrollModel(); 
        _mockMapper.Setup(m => m.Map<PayrollModel>(It.IsAny<Payroll>())).Returns(expectedModel);

        var result = _payrollService.GetCurrentPayroll(companyId);

        Assert.Equal(expectedModel, result);
    }

    [Fact]
    public void GetCurrentPayroll_ReturnsNull_WhenNoPayrollExists()
    {
        int companyId = 1;

        var payrollData = new List<Payroll> { }.AsQueryable();
     
        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var result = _payrollService.GetCurrentPayroll(companyId);

        Assert.Null(result);
    }
    
        [Fact]
        public void GetCurrentPayroll_ReturnsFirst_WhenMultipleScheduledPayrollsExist()
        {
            int companyId = 1;
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            var payrollData = new List<Payroll>()
            {
                new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-3), StartDate = today, TrueRunDate = null },
                new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-2), StartDate = today, TrueRunDate = null },
            }.AsQueryable();
        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

            var expectedModel = new PayrollModel { CompanyId = companyId, ScheduledRunDate =  today.AddDays(-3).ToDateTime(TimeOnly.MinValue), StartDate = today.ToDateTime(TimeOnly.MinValue), TrueRunDate = null }; 
        

    _mockMapper.Setup(m => m.Map<PayrollModel>(It.IsAny<Payroll>())).Returns(expectedModel);

            var result = _payrollService.GetCurrentPayroll(companyId);

            Assert.Equal(expectedModel, result); 
        }
    [Fact]
    public void GetPreviousPayroll_ReturnsMappedModel_WhenPreviousPayrollExists()
    {
        int companyId = 1;
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var payrollData = new List<Payroll>()
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-3) },
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-5) },
            new Payroll { CompanyId = companyId, ScheduledRunDate = today.AddDays(-2) },
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var expectedModel = new PayrollModel { CompanyId = companyId, ScheduledRunDate = today.AddDays(-5).ToDateTime(TimeOnly.MinValue), StartDate = today.ToDateTime(TimeOnly.MinValue), TrueRunDate = null };
        _mockMapper.Setup(m => m.Map<PayrollModel>(It.IsAny<Payroll>())).Returns(expectedModel);

        var result = _payrollService.GetPreviousPayroll(companyId);
       
        Assert.Equal(expectedModel, result);
    }
    [Fact]
    public void GetPreviousPayroll_ReturnsNull_WhenNoPreviousPayrollExists()
    {

        int companyId = 1;
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        var payrollData = new List<Payroll>()
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = today }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var result = _payrollService.GetPreviousPayroll(companyId);
        // Assert
        Assert.Null(result);
    }
    [Fact]
    public void GetPreviousPayroll_ReturnsNull_WhenNoPayrollExists()
    {

        int companyId = 1;
        var payrollData = new List<Payroll> { }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(payrollData.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var result = _payrollService.GetPreviousPayroll(companyId);

        Assert.Null(result);
    }
    [Fact]
    public void GetById_ReturnsMappedModel_WhenPayrollExists()
    {
        int companyId = 1;
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        var payrollData = new List<Payroll>()
        {
            new Payroll { CompanyId = companyId, ScheduledRunDate = today }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);

        var expectedPayrollModel = new PayrollModel { CompanyId = companyId, ScheduledRunDate = today.ToDateTime(TimeOnly.MinValue) };
             
        _mockMapper.Setup(m => m.Map<PayrollModel>(It.IsAny<Payroll>())).Returns(expectedPayrollModel);

        var result = _payrollService.GetById(companyId);

        Assert.Equal(expectedPayrollModel, result);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenPayrollDoesNotExist()
    {

        int id = 1;
        var payrollData = new List<Payroll>{ }.AsQueryable();

        var mockSet = new Mock<DbSet<Payroll>>();
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(payrollData.Provider);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(payrollData.Expression);
        mockSet.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(payrollData.ElementType);
        _mockContext.Setup(c => c.Payroll).Returns(mockSet.Object);
        var result = _payrollService.GetById(id);

        Assert.Null(result);
    }
    [Fact]
    public async Task CreateNextPayrollForAll_CreatesPayrolls_ForExistingWithScheduleChange()
    {
   

        int companyId1 = 1;
        int companyId2 = 2;

        var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2));

        // Mock PayrollSchedule data
        var existingSchedule1 = new PayrollSchedule { Id = 1, CompanyId = companyId1, FirstRunDate = startDate.AddDays(-10),PayrollScheduleTypeId=1};
        var existingSchedule2 = new PayrollSchedule { Id = 2, CompanyId = companyId2, FirstRunDate = startDate.AddDays(-15), PayrollScheduleTypeId = 2 };
        var existingSchedules=new List<PayrollSchedule>() { existingSchedule1,existingSchedule2 }.AsQueryable();

        var newSchedule1 = new PayrollSchedule { Id = 3, CompanyId = companyId1, FirstRunDate = startDate.AddDays(-5) , PayrollScheduleTypeId = 1};
        var newSchedule2 = new PayrollSchedule { Id = 4, CompanyId = companyId2, FirstRunDate = startDate ,PayrollScheduleTypeId=2};
        var newSchedules = new List<PayrollSchedule>() { newSchedule1, newSchedule2 }.AsQueryable();

        var mockPaySchedules = new Mock<DbSet<PayrollSchedule>>();
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Provider).Returns(existingSchedules.Provider);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.Expression).Returns(existingSchedules.Expression);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.ElementType).Returns(existingSchedules.ElementType);
        mockPaySchedules.As<IQueryable<PayrollSchedule>>().Setup(m => m.GetEnumerator()).Returns(existingSchedules.GetEnumerator());
        _mockContext.Setup(c => c.PayrollSchedule).Returns(mockPaySchedules.Object);

        // Mock current payroll data
        var currentPayroll1 = new Payroll { Id = 1, CompanyId = companyId1, ScheduledRunDate = startDate, PayrollSchedule = existingSchedule1 };
        var currentPayroll2 = new Payroll { Id = 2, CompanyId = companyId2, ScheduledRunDate = startDate.AddDays(-1), PayrollSchedule = existingSchedule2 };
        var currentPayrolls = new List<Payroll>() { currentPayroll1, currentPayroll2 }.AsQueryable();

        var mockPayroll = new Mock<DbSet<Payroll>>();
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.Provider).Returns(currentPayrolls.Provider);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.Expression).Returns(currentPayrolls.Expression);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.ElementType).Returns(currentPayrolls.ElementType);
        mockPayroll.As<IQueryable<Payroll>>().Setup(m => m.GetEnumerator()).Returns(currentPayrolls.GetEnumerator());
        _mockContext.Setup(c => c.Payroll).Returns(mockPayroll.Object);

        var addedPayrolls = new List<Payroll>();
        _mockContext.Setup(m => m.Payroll.AddRange(It.IsAny<IEnumerable<Payroll>>()))
           .Callback<IEnumerable<Payroll>>(payrolls => addedPayrolls.AddRange(payrolls));

        //var service = new PayrollService(mockContext.Object, mockMapper.Object);

        // Act
        var result = await _payrollService.CreateNextPayrollForAll();

        // Assert
        Assert.Equal(1, result); // Verify two new payrolls are created
       


        // Verify new payrolls are added with updated schedule and next run date
        _mockContext.Verify(m => m.Payroll.AddRange(It.IsAny<IEnumerable<Payroll>>()), Times.Once);



        // var addedPayrolls = _mockContext.Invocations.FirstOrDefault(i => i.Method.Name == "AddRange").Arguments[0] as IEnumerable<Payroll>;
        Assert.Single(addedPayrolls);
        var newPayroll1 = addedPayrolls.Single(p => p.CompanyId == companyId1);
        //var newPayroll2 = addedPayrolls.Single(p => p.CompanyId == companyId2);

        //var newPayroll1 = addedPayrolls.Where(p => p.CompanyId == companyId1).Single();
        //var newPayroll2 = addedPayrolls.Where(p => p.CompanyId == companyId2).Single();

        Assert.Equal(newSchedule1.Id, newPayroll1.PayrollScheduleId);
        Assert.Equal(startDate.AddDays(-1).AddMonths(1), newPayroll1.ScheduledRunDate); // Monthly with schedule change
        //Assert.Equal(newSchedule2.Id, newPayroll2.PayrollScheduleId);
        //Assert.Equal(startDate.AddDays(13), newPayroll2.ScheduledRunDate); // Semi-monthly

    }
    [Fact]
    public void CalculatePayForDailyRatedEmployees_ShouldReturnCorrectPayAndDaysWorked()
    {
        // Arrange
        var timecards = new List<TimecardUsa>
        {
            new TimecardUsa { PersonId = 1, ClockIn = new DateTime(2024, 10, 1, 9, 0, 0) },
            new TimecardUsa { PersonId = 1, ClockIn = new DateTime(2024, 10, 2, 9, 0, 0) },
            new TimecardUsa { PersonId = 1, ClockIn = new DateTime(2024, 10, 3, 9, 0, 0) }
        };
        var currentPayroll = new Payroll
        {
            StartDate = new DateOnly(2024, 10, 1),
            ScheduledRunDate = new DateOnly(2024, 10, 31)
        };
        var emp = new Employment { PersonId = 1, PayRate = 100 };

        // Act
        var methodInfo = typeof(PayrollService).GetMethod("CalculatePayForDailyRatedEmployees", BindingFlags.NonPublic | BindingFlags.Static);
        var result = (Tuple<double, int>)methodInfo.Invoke(null, new object[] { timecards, currentPayroll, emp });

        // Assert
        Assert.Equal(300, result.Item1); 
        Assert.Equal(3, result.Item2); 
    }
}
